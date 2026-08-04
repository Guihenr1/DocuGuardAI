using DocuGuardAI.Application.Common.Models;

namespace DocuGuardAI.Application.Common.Interfaces;

public interface IContentSafetyService
{
    Task<ContentSafetyResult> AnalyzeTextAsync(
        string text,
        IReadOnlyList<string>? blocklistNames = null,
        CancellationToken cancellationToken = default);
}