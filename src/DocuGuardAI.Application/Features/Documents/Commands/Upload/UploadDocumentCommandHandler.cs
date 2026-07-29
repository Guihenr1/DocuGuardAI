using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Commands.Upload;

public class UploadDocumentCommandHandler(IDocumentRepository documentRepository) 
    : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResponse>>
{
    public async Task<Result<UploadDocumentResponse>> Handle(
        UploadDocumentCommand request,
        CancellationToken ct)
    {
        var document = Document.Create(
            request.UserId,
            request.FileName,
            request.FilePath,
            request.ContentType,
            request.FileSize);

        await documentRepository.AddAsync(document, ct);

        return Result.Success(new UploadDocumentResponse(
            document.Id,
            document.Name,
            document.UploadedAt));
    }
}