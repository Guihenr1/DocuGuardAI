using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Interfaces.Repositories;

public interface ICompanyRepository
{
    Task AddAsync(Company company, CancellationToken ct);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Company>> GetAllAsync(CancellationToken ct);
    Task UpdateAsync(Company company, CancellationToken ct);
    Task DeleteAsync(Company company, CancellationToken ct);
    Task<Company?> GetByNameAsync(string name, CancellationToken ct);
}