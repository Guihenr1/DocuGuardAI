using DocuGuardAI.Domain.ValueObjects;

namespace DocuGuardAI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Document> Documents { get; set; } = [];

    public static User Create(string email, string hashPassword,  UserRole role = UserRole.Viewer)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(), 
            Email = email, 
            Role = role,
            PasswordHash = hashPassword,
            IsActive = true
        };
        
        return user;
    }
}