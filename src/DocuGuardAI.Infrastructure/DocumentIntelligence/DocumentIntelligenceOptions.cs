namespace DocuGuardAI.Infrastructure.DocumentIntelligence;

public class DocumentIntelligenceOptions
{
    public const string SectionName = "DocumentIntelligence";

    public string Endpoint { get; set; } = string.Empty;
    
    public string? Key { get; set; }

    /// <summary>
    /// Model to use. Recommended: "prebuilt-read" for pure text extraction.
    /// Alternatives: "prebuilt-layout", "prebuilt-document", etc.
    /// </summary>
    public string ModelId { get; set; } = "prebuilt-read";
}