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
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
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
            Status = DocumentStatus.Pending,
            IsActive = true
        };
    }
}

public enum DocumentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}