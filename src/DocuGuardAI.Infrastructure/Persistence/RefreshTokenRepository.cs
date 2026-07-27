using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DocuGuardAIDbContext _context;

    public RefreshTokenRepository(DocuGuardAIDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync(ct);
        
        foreach (var token in refreshTokens)
        {
            token.Revoke();
        }
        
        if (refreshTokens.Count != 0) await _context.SaveChangesAsync(ct);
    }
}