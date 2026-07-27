namespace DocuGuardAI.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    public static RefreshToken Create(Guid userId, DateTime expiresAt)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = expiresAt
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}