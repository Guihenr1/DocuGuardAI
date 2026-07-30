using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class CompanyRepository : ICompanyRepository
{
    private readonly DocuGuardAIDbContext _context;

    public CompanyRepository(DocuGuardAIDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Company company, CancellationToken ct)
    {
        await _context.Companies.AddAsync(company, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Companies.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Companies.AsNoTracking().ToListAsync(ct);
    }

    public async Task UpdateAsync(Company company, CancellationToken ct)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Company company, CancellationToken ct)
    {
        _context.Companies.Remove(company);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Company?> GetByNameAsync(string name, CancellationToken ct)
    {
        return await _context.Companies.FirstOrDefaultAsync(c => c.Name == name, ct);
    }
}