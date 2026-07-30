namespace DocuGuardAI.Application.Features.Companies;

public sealed class CompanyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? AdministratorId { get; init; }
    public DocuGuardAI.Domain.Entities.CompanyAdminType AdministratorType { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }

    public CompanyDto() { }

    public CompanyDto(Guid id, string name, Guid? administratorId, DocuGuardAI.Domain.Entities.CompanyAdminType administratorType, DateTime createdAt, bool isActive)
    {
        Id = id; Name = name; AdministratorId = administratorId; AdministratorType = administratorType; CreatedAt = createdAt; IsActive = isActive;
    }
}