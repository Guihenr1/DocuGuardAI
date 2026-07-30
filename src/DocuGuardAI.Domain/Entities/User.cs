using System;
using System.Collections.Generic;
using DocuGuardAI.Domain.ValueObjects;

namespace DocuGuardAI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public static User Create(string email, string hashPassword, Guid companyId, UserRole role)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = role,
            PasswordHash = hashPassword,
            IsActive = true,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        return user;
    }
}