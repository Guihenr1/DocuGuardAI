namespace DocuGuardAI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public static User Create(string email, string hashPassword)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(), 
            Email = email, 
            PasswordHash = hashPassword,
            IsActive = true
        };
        
        return user;
    }
}