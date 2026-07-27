using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
}