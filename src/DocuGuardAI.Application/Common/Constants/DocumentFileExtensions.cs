namespace DocuGuardAI.Application.Common.Constants;

public static class DocumentFileExtensions
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".heif",
        ".docx", ".xlsx", ".pptx",
        ".html", ".htm"
    };

    public static bool IsAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && Allowed.Contains(extension);
    }
}