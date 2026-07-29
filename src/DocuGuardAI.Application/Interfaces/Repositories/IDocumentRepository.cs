using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Document>> GetUserDocumentsAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Document>> GetDocumentsByStatusAsync(DocumentStatus status, CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
    Task UpdateAsync(Document document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}