using Azure;
using Azure.AI.ContentSafety;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Infrastructure.ContentSafety;

public class AzureContentSafetyService(
    ContentSafetyClient client,
    IOptions<ContentSafetyOptions> options)
    : IContentSafetyService
{
    private readonly ContentSafetyOptions _options = options.Value;

    public async Task<ContentSafetyResult> AnalyzeTextAsync(
        string text,
        IReadOnlyList<string>? blocklistNames = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ContentSafetyResult(
                IsSafe: true,
                CategorySeverities: new Dictionary<string, int>());
        }

        var request = new AnalyzeTextOptions(text)
        {
            OutputType = AnalyzeTextOutputType.FourSeverityLevels
        };

        if (blocklistNames is { Count: > 0 })
        {
            foreach (var name in blocklistNames)
                request.BlocklistNames.Add(name);

            request.HaltOnBlocklistHit = _options.HaltOnBlocklistHit;
        }

        Response<AnalyzeTextResult> response =
            await client.AnalyzeTextAsync(request, cancellationToken);

        var severities = response.Value.CategoriesAnalysis
            .ToDictionary(
                c => c.Category.ToString(),
                c => c.Severity ?? 0);

        var matches = response.Value.BlocklistsMatch?
            .Select(m => new BlocklistMatch(
                m.BlocklistName,
                m.BlocklistItemId,
                m.BlocklistItemText))
            .ToList();

        var isSafe = severities.Values.All(s => s < _options.SeverityThreshold)
                     && (matches is null || matches.Count == 0);

        return new ContentSafetyResult(isSafe, severities, matches);
    }
}