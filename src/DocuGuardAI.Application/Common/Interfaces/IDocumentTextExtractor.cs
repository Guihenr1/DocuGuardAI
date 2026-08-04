namespace DocuGuardAI.Application.Common.Interfaces;

public interface IDocumentTextExtractor
{
    Task<string> ExtractAsync(
        string filePath, 
        string contentType, 
        CancellationToken cancellationToken = default);
}