using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Common.Interfaces;

public interface IChatCompletionService
{
    Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        string? documentContext = null,
        CancellationToken cancellationToken = default);
}