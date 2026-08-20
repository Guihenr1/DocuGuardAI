using System.Security.Cryptography;
using Azure;
using Azure.AI.DocumentIntelligence;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Infrastructure.DocumentIntelligence;

public class DocumentIntelligenceTextExtractor(
    DocumentIntelligenceClient client,
    ICacheService cache,
    IOptions<DocumentIntelligenceOptions> options,
    ILogger<DocumentIntelligenceTextExtractor> logger)
    : IDocumentTextExtractor
{
    private readonly DocumentIntelligenceOptions _options = options.Value;

    public async Task<string> ExtractAsync(
        string filePath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document file not found.", filePath);
        
        var fileHash = await ComputeFileHashAsync(filePath, cancellationToken);
        var cacheKey = $"docintel:text:{_options.ModelId}:{fileHash}";

        var cached = await cache.GetAsync<string>(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogInformation("Cache hit for document {FilePath}", filePath);
            return cached;
        }

        logger.LogInformation(
            "Extracting text from {FilePath} ({ContentType}) using model {ModelId}",
            filePath, contentType, _options.ModelId);

        await using var stream = File.OpenRead(filePath);

        var operation = await client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _options.ModelId,
            BinaryData.FromStream(stream),
            cancellationToken);

        var result = operation.Value;

        if (!string.IsNullOrWhiteSpace(result.Content))
        {
            await cache.SetAsync(cacheKey, result.Content.Trim(), TimeSpan.FromHours(24), cancellationToken);
            return result.Content.Trim();
        }

        var textBuilder = new System.Text.StringBuilder();

        foreach (var page in result.Pages)
        {
            if (page.Lines is null) continue;

            foreach (var line in page.Lines)
            {
                textBuilder.AppendLine(line.Content);
            }

            textBuilder.AppendLine();
        }

        var extracted = textBuilder.ToString().Trim();
        
        await cache.SetAsync(cacheKey, extracted, TimeSpan.FromHours(24), cancellationToken);

        logger.LogInformation(
            "Extracted {Length} characters from {FilePath}",
            extracted.Length, filePath);

        return extracted;
    }
    
    private static async Task<string> ComputeFileHashAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes);
    }
}