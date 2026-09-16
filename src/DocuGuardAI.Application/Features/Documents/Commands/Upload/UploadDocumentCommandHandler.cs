using Ardalis.Result;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Common.Models;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
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

        // 1. Extract text
        var text = await textExtractor.ExtractAsync(
            request.FilePath,
            request.ContentType,
            ct);

        document.MarkTextExtracted();
        await documentRepository.UpdateAsync(document, ct);

        // 2. Preprocess text
        var cleanedText = textPreprocessor.Preprocess(text);

        // 3. Content Safety check
        var safetyResult = await contentSafety.AnalyzeTextAsync(
            cleanedText,
            cancellationToken: ct);

        if (!safetyResult.IsSafe)
        {
            document.MarkUnprocessed(BuildSafetySummary(safetyResult));
            await documentRepository.UpdateAsync(document, ct);

            await SaveDocumentMemoryAsync(document, cleanedText, isSafe: false, ct);

            return Result.Success(new UploadDocumentResponse(
                document.Id,
                document.Name,
                document.UploadedAt,
                document.Status,
                document.ProcessingResult));
        }

        // 4. Safety passed
        document.MarkSafetyCheckPassed();
        
        var summary = cleanedText.Length > 3000 
            ? cleanedText.Substring(0, 3000) + "..." 
            : cleanedText;

        document.SetSummary(summary);
        
        await documentRepository.UpdateAsync(document, ct);

        await SaveDocumentMemoryAsync(document, cleanedText, isSafe: true, ct);

        // TODO: publish event / enqueue next pipeline steps here

        return Result.Success(new UploadDocumentResponse(
            document.Id,
            document.Name,
            document.UploadedAt,
            document.Status,
            "Document uploaded successfully and is ready for conversation."));
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