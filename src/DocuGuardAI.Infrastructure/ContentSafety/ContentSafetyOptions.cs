namespace DocuGuardAI.Infrastructure.ContentSafety;

public class ContentSafetyOptions
{
    public const string SectionName = "ContentSafety";

    public string Endpoint { get; set; } = string.Empty;
    public int SeverityThreshold { get; set; } = 4;
    public string? Key { get; set; }
    public bool HaltOnBlocklistHit { get; set; } = true;
}