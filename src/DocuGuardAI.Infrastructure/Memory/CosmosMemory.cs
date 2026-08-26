using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Infrastructure.Memory;

public class CosmosMemory : ICosmosMemory
{
    private readonly Container _container;
    private readonly ILogger<CosmosMemory> _logger;

    public CosmosMemory(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> options,
        ILogger<CosmosMemory> logger)
    {
        var opts = options.Value;
        _logger = logger;
        
        EnsureDatabaseAndContainerAsync(cosmosClient, opts)
            .GetAwaiter()
            .GetResult();

        _container = cosmosClient.GetContainer(opts.DatabaseName, opts.ContainerName);
    }

    private static async Task EnsureDatabaseAndContainerAsync(
        CosmosClient cosmosClient, 
        CosmosOptions options)
    {
        var databaseResponse = await cosmosClient
            .CreateDatabaseIfNotExistsAsync(options.DatabaseName);

        await databaseResponse.Database.CreateContainerIfNotExistsAsync(
            id: options.ContainerName,
            partitionKeyPath: "/userId");
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<UserProfile>(
                id: userId,
                partitionKey: new PartitionKey(userId),
                cancellationToken: ct);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task SaveUserProfileAsync(UserProfile profile, CancellationToken ct = default)
    {
        profile.Id = profile.UserId;
        profile.Type = "userProfile";
        profile.LastUpdated = DateTime.UtcNow;

        await _container.UpsertItemAsync(
            item: profile,
            partitionKey: new PartitionKey(profile.UserId),
            cancellationToken: ct);

        _logger.LogInformation("User profile saved for {UserId}", profile.UserId);
    }

    public async Task SaveDocumentMemoryAsync(DocumentMemory memory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(memory.Id))
            memory.Id = Guid.NewGuid().ToString();

        memory.Type = "document";
        memory.AnalyzedAt = DateTime.UtcNow;

        await _container.UpsertItemAsync(
            item: memory,
            partitionKey: new PartitionKey(memory.UserId),
            cancellationToken: ct);

        _logger.LogInformation("Document memory saved: {FileName} for {UserId}", 
            memory.FileName, memory.UserId);
    }

    public async Task<IReadOnlyList<DocumentMemory>> GetDocumentsByUserAsync(
        string userId, 
        int limit = 10, 
        CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.type = 'document' ORDER BY c.analyzedAt DESC OFFSET 0 LIMIT @limit")
            .WithParameter("@userId", userId)
            .WithParameter("@limit", limit);

        var results = new List<DocumentMemory>();

        using var iterator = _container.GetItemQueryIterator<DocumentMemory>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(userId)
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task SaveFactAsync(string userId, string fact, CancellationToken ct = default)
    {
        var memoryFact = new MemoryFact
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Content = fact,
            Type = "fact"
        };

        await _container.UpsertItemAsync(
            item: memoryFact,
            partitionKey: new PartitionKey(userId),
            cancellationToken: ct);
    }

    public async Task<IReadOnlyList<string>> GetRelevantFactsAsync(
        string userId, 
        int limit = 20, 
        CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT c.content FROM c WHERE c.userId = @userId AND c.type = 'fact' ORDER BY c.createdAt DESC OFFSET 0 LIMIT @limit")
            .WithParameter("@userId", userId)
            .WithParameter("@limit", limit);

        var results = new List<string>();

        using var iterator = _container.GetItemQueryIterator<MemoryFact>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(userId)
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response.Select(f => f.Content));
        }

        return results;
    }
}