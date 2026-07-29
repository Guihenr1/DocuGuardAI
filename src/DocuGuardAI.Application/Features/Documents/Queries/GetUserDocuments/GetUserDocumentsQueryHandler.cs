using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Queries.GetUserDocuments;

public class GetUserDocumentsQueryHandler(IDocumentRepository documentRepository)
    : IRequestHandler<GetUserDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetUserDocumentsQuery request,
        CancellationToken ct)
    {
        var documents = await documentRepository.GetUserDocumentsAsync(request.UserId, ct);

        var dtos = documents.Select(d => new DocumentDto(
            d.Id,
            d.Name,
            d.FileSize,
            d.UploadedAt,
            d.Status.ToString(),
            d.IsActive,
            d.ProcessedAt));

        return Result.Success(dtos);
    }
}