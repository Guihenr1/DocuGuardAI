using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class DocumentRepository(DocuGuardAIDbContext context) : IDocumentRepository
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Documents
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IEnumerable<Document>> GetUserDocumentsAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Document>> GetDocumentsByStatusAsync(DocumentStatus status, CancellationToken ct = default)
    {
        return await context.Documents
            .Where(d => d.Status == status)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Document document, CancellationToken ct = default)
    {
        await context.Documents.AddAsync(document, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        context.Documents.Update(document);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        if (document != null)
        {
            document.IsActive = false;
            context.Documents.Update(document);
            await context.SaveChangesAsync(ct);
        }
    }
}