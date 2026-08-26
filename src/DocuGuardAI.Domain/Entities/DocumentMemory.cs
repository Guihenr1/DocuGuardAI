namespace DocuGuardAI.Domain.Entities;

public class DocumentMemory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = "document";
    public string FileName { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public Dictionary<string, string> KeyValues { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}