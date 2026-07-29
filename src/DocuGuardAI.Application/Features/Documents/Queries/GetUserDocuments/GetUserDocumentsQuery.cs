using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Queries.GetUserDocuments;

public record GetUserDocumentsQuery(Guid UserId) : IRequest<Result<IEnumerable<DocumentDto>>>;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Parameterless ctor for System.Text.Json
    public DocumentDto() { }

    public DocumentDto(Guid id, string name, long fileSize, DateTime uploadedAt, string status, bool isActive, DateTime? processedAt)
    {
        Id = id;
        Name = name;
        FileSize = fileSize;
        UploadedAt = uploadedAt;
        Status = status;
        IsActive = isActive;
        ProcessedAt = processedAt;
    }
}