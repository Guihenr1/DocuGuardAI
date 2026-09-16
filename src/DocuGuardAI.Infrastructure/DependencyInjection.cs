using Azure;
using Azure.AI.ContentSafety;
using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Infrastructure.Auth;
using DocuGuardAI.Infrastructure.Caching;
using DocuGuardAI.Infrastructure.ContentSafety;
using DocuGuardAI.Infrastructure.DocumentIntelligence;
using DocuGuardAI.Infrastructure.Memory;
using DocuGuardAI.Infrastructure.NaturalLanguageProcessing;
using DocuGuardAI.Infrastructure.Persistence;
using DocuGuardAI.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddScoped<IContentSafetyService, AzureContentSafetyService>();
        services.AddScoped<ICosmosMemory, CosmosMemory>();
        services.AddSingleton<ITextPreprocessor, TextPreprocessor>();
        
        services.Configure<AzureOpenAISettings>(
            configuration.GetSection("AzureOpenAI"));

        services.AddScoped<IChatCompletionService, AzureOpenAIChatCompletionService>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

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
        
        services.AddMemoryCache();
        services.AddSingleton<ICacheService>(sp =>
        {
            var redisConnectionString = sp.GetRequiredService<IConfiguration>();
            var redisConnection = redisConnectionString.GetConnectionString("Redis");

            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                var distributedCache = sp.GetService<IDistributedCache>();

                if (distributedCache != null)
                {
                    var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
                    return new RedisCacheService(distributedCache, logger);
                }
            }

            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            return new LocalCacheService(memoryCache);
        });
        
        services.Configure<CosmosOptions>(
            configuration.GetSection(CosmosOptions.SectionName));
        
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            if (string.IsNullOrWhiteSpace(options.Endpoint))
                throw new InvalidOperationException("CosmosDb:Endpoint is missing.");

            if (!string.IsNullOrWhiteSpace(options.Key))
            {
                return new CosmosClient(
                    options.Endpoint,
                    options.Key,
                    clientOptions);
            }

            return new CosmosClient(
                options.Endpoint,
                new DefaultAzureCredential(),
                clientOptions);
        });

        services.AddHttpContextAccessor();

        return services;
    }
}