using Azure;
using Azure.AI.DocumentIntelligence;
using DocuGuardAI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Infrastructure.DocumentIntelligence;

public class DocumentIntelligenceTextExtractor(
    DocumentIntelligenceClient client,
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
            return result.Content.Trim();

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

        logger.LogInformation(
            "Extracted {Length} characters from {FilePath}",
            extracted.Length, filePath);

        return extracted;
    }
}