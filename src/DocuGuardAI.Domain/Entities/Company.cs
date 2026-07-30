using System;
using System.Collections.Generic;

namespace DocuGuardAI.Domain.Entities;

public enum CompanyAdminType
{
    Primary = 0,
    Secondary = 1
}

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? AdministratorId { get; set; }
    public CompanyAdminType AdministratorType { get; set; } = CompanyAdminType.Primary;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public User? Administrator { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public static Company Create(string name, Guid? adminId = null, CompanyAdminType adminType = CompanyAdminType.Primary)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            AdministratorId = adminId,
            AdministratorType = adminType,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}