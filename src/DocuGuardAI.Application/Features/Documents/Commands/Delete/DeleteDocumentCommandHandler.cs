using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Commands.Delete;

public class DeleteDocumentCommandHandler(IDocumentRepository documentRepository)
    : IRequestHandler<DeleteDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, ct);

        if (document == null)
            return Result.NotFound($"Document {request.DocumentId} not found");

        if (document.UserId != request.UserId)
            return Result.Forbidden("You don't have permission to delete this document");
        
        if (!document.IsActive)
            return Result.Forbidden("This document is not active");

        await documentRepository.DeleteAsync(request.DocumentId, ct);

        return Result.Success();
    }
}