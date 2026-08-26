using Ardalis.Result;
using DocuGuardAI.Application.Common.Interfaces;       
using DocuGuardAI.Application.Common.Models;     
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.Enums;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Commands.Upload;

public class UploadDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IUserRepository userRepository,
    IDocumentTextExtractor textExtractor,
    IContentSafetyService contentSafety,
    ITextPreprocessor textPreprocessor,
    ICosmosMemory cosmosMemory) 
    : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResponse>>
{
    public async Task<Result<UploadDocumentResponse>> Handle(
        UploadDocumentCommand request,
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.NotFound("User not found.");

        var document = Document.Create(
            request.UserId,
            user.CompanyId,
            request.FileName,
            request.FilePath,
            request.ContentType,
            request.FileSize);

        await documentRepository.AddAsync(document, ct);

        var text = await textExtractor.ExtractAsync(
            request.FilePath, 
            request.ContentType, 
            ct);
        
        var cleanedText = textPreprocessor.Preprocess(text);

        var safetyResult = await contentSafety.AnalyzeTextAsync(cleanedText, cancellationToken: ct);

        if (!safetyResult.IsSafe)
        {
            document.Status = DocumentStatus.Unprocessed;
            document.ProcessingResult = BuildSafetySummary(safetyResult);
            document.ProcessedAt = DateTime.UtcNow;

            await documentRepository.UpdateAsync(document, ct);
            
            await SaveDocumentMemoryAsync(document, cleanedText, isSafe: false, ct);

            return Result.Success(new UploadDocumentResponse(
                document.Id,
                document.Name,
                document.UploadedAt,
                IsSafe: false,
                SafetyMessage: document.ProcessingResult));
        }

        document.Status = DocumentStatus.SafetyCheckPassed;
        document.ProcessingResult = "Content safety check passed";

        await documentRepository.UpdateAsync(document, ct);
        
        await SaveDocumentMemoryAsync(document, cleanedText, isSafe: true, ct);

        // publish event / enqueue next pipeline steps here

        return Result.Success(new UploadDocumentResponse(
            document.Id,
            document.Name,
            document.UploadedAt,
            IsSafe: true));
    }
    
    private async Task SaveDocumentMemoryAsync(
        Document document, 
        string cleanedText, 
        bool isSafe, 
        CancellationToken ct)
    {
        var memory = new DocumentMemory
        {
            UserId = document.UserId.ToString(),
            FileName = document.Name,
            Summary = isSafe 
                ? $"Document passed safety check. Length: {cleanedText.Length} chars" 
                : document.ProcessingResult,
            KeyValues = new Dictionary<string, string>
            {
                ["Status"] = document.Status.ToString(),
                ["ContentType"] = document.ContentType ?? string.Empty,
                ["DocumentId"] = document.Id.ToString()
            }
        };

        await cosmosMemory.SaveDocumentMemoryAsync(memory, ct);
    }

    private static string BuildSafetySummary(ContentSafetyResult result)
    {
        var severities = string.Join(", ",
            result.CategorySeverities.Select(kv => $"{kv.Key}={kv.Value}"));

        var matches = result.BlocklistMatches is { Count: > 0 }
            ? " | Blocklist hits: " + string.Join(", ", result.BlocklistMatches.Select(m => m.Text))
            : string.Empty;

        return $"Rejected by Content Safety. Severities: {severities}{matches}";
    }
}