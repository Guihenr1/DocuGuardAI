using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class DocuGuardAIDbContext(DbContextOptions<DocuGuardAIDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocuGuardAIDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}