using DocuGuardAI.Domain.Enums;

namespace DocuGuardAI.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public string? ProcessingResult { get; set; }
    public bool IsActive { get; set; }
    
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;

    public static Document Create(Guid userId, Guid companyId, string fileName, string filePath, string contentType, long fileSize)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyId = companyId,
            Name = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow,
            Status = DocumentStatus.Uploaded,
            IsActive = true
        };
    }
    
    public void MarkUnprocessed(string reason)
    {
        Status = DocumentStatus.Unprocessed;
        ProcessingResult = reason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkSafetyCheckPassed()
    {
        Status = DocumentStatus.SafetyCheckPassed;
    }
}
