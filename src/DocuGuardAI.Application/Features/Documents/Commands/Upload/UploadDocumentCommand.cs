using Ardalis.Result;
using DocuGuardAI.Domain.Enums;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Commands.Upload;

public record UploadDocumentCommand(
    Guid UserId,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSize
) : IRequest<Result<UploadDocumentResponse>>;

public record UploadDocumentResponse(
    Guid DocumentId,
    string FileName,
    DateTime UploadedAt,
    DocumentStatus Status,
    string? Message = null
);