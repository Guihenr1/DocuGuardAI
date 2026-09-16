using DocuGuardAI.Domain.Enums;

namespace DocuGuardAI.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DocumentStatus Status { get; private set; } = DocumentStatus.Uploaded;
    public string? ProcessingResult { get; private set; }
    public string? Summary { get; private set; }
    public bool IsActive { get; private set; } = true;

    public User User { get; private set; } = null!;
    public Company Company { get; private set; } = null!;

    // Required by EF Core
    private Document() { }

    public static Document Create(
        Guid userId,
        Guid companyId,
        string fileName,
        string filePath,
        string contentType,
        long fileSize)
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

    public void MarkTextExtracted()
    {
        Status = DocumentStatus.TextExtracted;
    }

    public void MarkSafetyCheckPassed()
    {
        Status = DocumentStatus.SafetyCheckPassed;
    }

    public void MarkUnprocessed(string reason)
    {
        Status = DocumentStatus.Unprocessed;
        ProcessingResult = reason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        if (Status is not (DocumentStatus.SafetyCheckPassed or DocumentStatus.Processing))
        {
            throw new InvalidOperationException(
                $"Cannot move document to Processing from status {Status}");
        }

        Status = DocumentStatus.Processing;
    }

    public void MarkCompleted(string? summary = null)
    {
        Status = DocumentStatus.Completed;
        Summary = summary;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = DocumentStatus.Failed;
        ProcessingResult = reason;
        ProcessedAt = DateTime.UtcNow;
    }
    
    public void MarkDeleted() => IsActive = false;

    public bool CanBeUsedInConversation()
    {
        return Status is DocumentStatus.SafetyCheckPassed
                     or DocumentStatus.Processing
                     or DocumentStatus.Completed;
    }
    
    public void SetSummary(string summary)
    {
        Summary = summary;
    }
}