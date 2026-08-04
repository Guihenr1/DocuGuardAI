using Azure;
using Azure.AI.ContentSafety;
using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Infrastructure.Auth;
using DocuGuardAI.Infrastructure.ContentSafety;
using DocuGuardAI.Infrastructure.DocumentIntelligence;
using DocuGuardAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!);

        services.AddDbContext<DocuGuardAIDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsql =>
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IDocumentTextExtractor, DocumentIntelligenceTextExtractor>();

        services.Configure<ContentSafetyOptions>(
            configuration.GetSection(ContentSafetyOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ContentSafetyOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.Endpoint))
                throw new InvalidOperationException("ContentSafety:Endpoint is missing in configuration.");
            
            if (!string.IsNullOrWhiteSpace(options.Key))
            {
                return new ContentSafetyClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.Key));
            }

            return new ContentSafetyClient(
                new Uri(options.Endpoint),
                new DefaultAzureCredential());
        });
        
        services.Configure<DocumentIntelligenceOptions>(
            configuration.GetSection(DocumentIntelligenceOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DocumentIntelligenceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.Endpoint))
                throw new InvalidOperationException("DocumentIntelligence:Endpoint is missing.");
            
            if (!string.IsNullOrWhiteSpace(options.Key))
            {
                return new DocumentIntelligenceClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.Key));
            }
            
            return new DocumentIntelligenceClient(new Uri(options.Endpoint), new DefaultAzureCredential());
        });

        services.AddScoped<IContentSafetyService, AzureContentSafetyService>();

        services.AddHttpContextAccessor();

        return services;
    }
}