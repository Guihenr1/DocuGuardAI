using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Common.Interfaces;

public interface ICosmosMemory
{
    Task<UserProfile?> GetUserProfileAsync(string userId, CancellationToken ct = default);
    Task SaveUserProfileAsync(UserProfile profile, CancellationToken ct = default);

    Task SaveDocumentMemoryAsync(DocumentMemory memory, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentMemory>> GetDocumentsByUserAsync(
        string userId, 
        int limit = 10, 
        CancellationToken ct = default);

    Task SaveFactAsync(string userId, string fact, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRelevantFactsAsync(
        string userId, 
        int limit = 20, 
        CancellationToken ct = default);
}