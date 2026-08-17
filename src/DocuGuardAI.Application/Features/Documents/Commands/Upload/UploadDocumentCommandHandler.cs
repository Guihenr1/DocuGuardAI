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
    ITextPreprocessor textPreprocessor) 
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

        // publish event / enqueue next pipeline steps here

        return Result.Success(new UploadDocumentResponse(
            document.Id,
            document.Name,
            document.UploadedAt,
            IsSafe: true));
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