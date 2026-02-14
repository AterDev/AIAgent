using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;
using Entity.KnowledgeBaseMod;

namespace CoreMod.Services.VectorStore;

/// <summary>
/// Qdrant vector database service for RAG document embedding and search
/// Uses Aspire Qdrant Client integration for dependency injection and automatic configuration
/// </summary>
public class QdrantService(
    QdrantClient client,
    IUserContext userContext,
    CoreModelEmbeddingGenerator embeddingGenerator,
    IOptions<QdrantOptions> options,
    ILogger<QdrantService> logger
) : IVectorStore
{
    private readonly QdrantOptions _options = options.Value;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public async Task<Dictionary<Guid, string>> UpsertAsync(RagDocument document, IReadOnlyList<RagChunk> chunks, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        try
        {
            var points = new List<PointStruct>(chunks.Count);
            foreach (var chunk in chunks)
            {
                var vector = await embeddingGenerator.GenerateAsync(chunk.Content, _options.VectorSize, cancellationToken);
                points.Add(new PointStruct
                {
                    Id = new PointId { Uuid = chunk.Id.ToString() },
                    Vectors = vector,
                    Payload =
                    {
                        ["tenantId"] = userContext.TenantId.ToString(),
                        ["documentId"] = document.Id.ToString(),
                        ["collectionId"] = document.CollectionId.ToString(),
                        ["chunkId"] = chunk.Id.ToString(),
                    },
                });
            }

            await client.UpsertAsync(
                collectionName: _options.CollectionName,
                points: points,
                wait: true,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Successfully upserted {ChunkCount} chunks for document {DocumentId}", chunks.Count, document.Id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to upsert chunks for document {DocumentId}", document.Id);
        }

        return chunks.ToDictionary(c => c.Id, c => c.Id.ToString());
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Guid? collectionId, string query, int topK, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        try
        {
            var vector = await embeddingGenerator.GenerateAsync(query, _options.VectorSize, cancellationToken);
            var filter = BuildFilter(collectionId);

            var searchResults = await client.SearchAsync(
                collectionName: _options.CollectionName,
                vector: new ReadOnlyMemory<float>(vector),
                filter: filter,
                limit: (ulong)topK,
                cancellationToken: cancellationToken
            );

            var results = new List<VectorSearchResult>();
            foreach (var scoredPoint in searchResults)
            {
                if (Guid.TryParse(scoredPoint.Id.Uuid, out var chunkId))
                {
                    results.Add(new VectorSearchResult(chunkId, (double)scoredPoint.Score));
                }
            }

            logger.LogInformation("Search completed, found {ResultCount} results for query", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Qdrant search failed");
            return [];
        }
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        try
        {
            var filter = MatchKeyword("tenantId", userContext.TenantId.ToString()) &
                        MatchKeyword("documentId", documentId.ToString());

            await client.DeleteAsync(
                collectionName: _options.CollectionName,
                filter: filter,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Successfully deleted vectors for document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete vectors for document {DocumentId}", documentId);
        }
    }

    public async Task DeleteChunksAsync(IEnumerable<Guid> chunkIds, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        try
        {
            await client.DeleteAsync(
                collectionName: _options.CollectionName,
                ids: chunkIds.ToList(),
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Successfully deleted {ChunkCount} chunks", chunkIds.Count());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete chunks");
        }
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                await client.GetCollectionInfoAsync(
                    collectionName: _options.CollectionName,
                    cancellationToken: cancellationToken
                );
                logger.LogInformation("Collection {CollectionName} exists", _options.CollectionName);
            }
            catch
            {
                try
                {
                    var vectorParams = new VectorParams
                    {
                        Size = (uint)_options.VectorSize,
                        Distance = Distance.Cosine,
                        OnDisk = false,
                    };

                    await client.CreateCollectionAsync(
                        collectionName: _options.CollectionName,
                        vectorsConfig: vectorParams,
                        shardNumber: 1,
                        replicationFactor: 1,
                        cancellationToken: cancellationToken
                    );

                    logger.LogInformation("Created collection {CollectionName}", _options.CollectionName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create collection {CollectionName}", _options.CollectionName);
                    throw;
                }
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private Filter? BuildFilter(Guid? collectionId)
    {
        var filter = MatchKeyword("tenantId", userContext.TenantId.ToString());

        if (collectionId.HasValue)
        {
            filter = filter & MatchKeyword("collectionId", collectionId.Value.ToString());
        }

        return filter;
    }
}