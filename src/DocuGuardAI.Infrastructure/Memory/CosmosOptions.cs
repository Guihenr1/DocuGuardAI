namespace DocuGuardAI.Infrastructure.Memory;

public class CosmosOptions
{
    public const string SectionName = "CosmosDb";

    public string Endpoint { get; set; } = string.Empty;
    public string? Key { get; set; }
    public string DatabaseName { get; set; } = "DocuGuardAI";
    public string ContainerName { get; set; } = "Memory";
}