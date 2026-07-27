using DocuGuardAI.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DocuGuardAI.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<IdentityUser> _hasher = new PasswordHasher<IdentityUser>();

    public string HashPassword(string password)
        => _hasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string hash)
        => _hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}