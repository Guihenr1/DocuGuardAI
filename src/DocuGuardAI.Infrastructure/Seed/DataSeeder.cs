using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuGuardAI.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task EnsureSeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var companyRepo = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var adminEmail = config["Seed:AdminEmail"] ?? "admin@docuguard.ai";
        var adminPassword = config["Seed:AdminPassword"] ?? "Password123!";
        var companyName = config["Seed:CompanyName"] ?? "DocuGuardAI";

        var company = await companyRepo.GetByNameAsync(companyName, ct);
        if (company == null)
        {
            company = Company.Create(companyName, null, CompanyAdminType.Primary);
            await companyRepo.AddAsync(company, ct);
        }

        var existingUser = await userRepo.GetByEmailAsync(adminEmail, ct);
        if (existingUser == null)
        {
            var adminUser = User.Create(Email.From(adminEmail), passwordHasher.HashPassword(adminPassword), company.Id, UserRole.SystemAdmin);
            await userRepo.AddAsync(adminUser, ct);

            if (company.AdministratorId == null || company.AdministratorId == Guid.Empty)
            {
                company.AdministratorId = adminUser.Id;
                await companyRepo.UpdateAsync(company, ct);
            }
        }
    }
}