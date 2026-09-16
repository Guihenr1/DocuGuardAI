using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class DocuGuardAIDbContext(DbContextOptions<DocuGuardAIDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Conversation> Conversations => Set<Conversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocuGuardAIDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}