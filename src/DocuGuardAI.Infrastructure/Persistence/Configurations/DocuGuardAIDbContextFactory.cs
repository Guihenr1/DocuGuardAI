using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DocuGuardAI.Infrastructure.Persistence.Configurations;

public class DocuGuardAIDbContextFactory : IDesignTimeDbContextFactory<DocuGuardAIDbContext>
{
    public DocuGuardAIDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<DocuGuardAIDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DocuGuardAIDbContext(optionsBuilder.Options);
    }
}