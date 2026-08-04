namespace DocuGuardAI.Application.Common.Models;

public sealed record ContentSafetyResult(
    bool IsSafe,
    IReadOnlyDictionary<string, int> CategorySeverities,
    IReadOnlyList<BlocklistMatch>? BlocklistMatches = null);