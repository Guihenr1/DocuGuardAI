using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly DocuGuardAIDbContext _context;

    public UserRepository(DocuGuardAIDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users.FindAsync(id, ct);
    }
}