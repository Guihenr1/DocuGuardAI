namespace DocuGuardAI.Domain.Entities;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = "userProfile";
    public string? DisplayName { get; set; }
    public Dictionary<string, string> Preferences { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}