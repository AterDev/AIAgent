using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace KnowledgeBaseMod.Services;

public class QdrantVectorStore(
    IHttpClientFactory httpClientFactory,
    IUserContext userContext,
    IEmbeddingGenerator embeddingGenerator,
    IOptions<QdrantOptions> options,
    ILogger<QdrantVectorStore> logger
) : IVectorStore
{
    private readonly QdrantOptions _options = options.Value;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public async Task<Dictionary<Guid, string>> UpsertAsync(RagDocument document, IReadOnlyList<RagChunk> chunks, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        var points = chunks.Select(chunk => new
        {
            id = chunk.Id,
            vector = embeddingGenerator.Generate(chunk.Content, _options.VectorSize),
            payload = new Dictionary<string, object>
            {
                ["tenantId"] = userContext.TenantId.ToString(),
                ["documentId"] = document.Id.ToString(),
                ["collectionId"] = document.CollectionId.ToString(),
                ["chunkId"] = chunk.Id.ToString(),
            },
        }).ToList();

        var body = new
        {
            points,
        };

        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/collections/{_options.CollectionName}/points?wait=true", body, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Qdrant upsert failed: {Status} {Body}", response.StatusCode, content);
        }

        return chunks.ToDictionary(c => c.Id, c => c.Id.ToString());
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Guid? collectionId, string query, int topK, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        var vector = embeddingGenerator.Generate(query, _options.VectorSize);
        var filter = new
        {
            must = BuildFilter(collectionId),
        };

        var body = new
        {
            vector,
            limit = topK,
            filter,
            with_payload = false,
        };

        var client = CreateClient();
        using var response = await client.PostAsJsonAsync($"/collections/{_options.CollectionName}/points/search", body, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Qdrant search failed: {Status} {Body}", response.StatusCode, content);
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("result", out var resultElement) || resultElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var list = new List<VectorSearchResult>();
        foreach (var item in resultElement.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idElement))
            {
                continue;
            }

            Guid chunkId;
            if (idElement.ValueKind == JsonValueKind.String && Guid.TryParse(idElement.GetString(), out var idGuid))
            {
                chunkId = idGuid;
            }
            else
            {
                continue;
            }

            var score = item.TryGetProperty("score", out var scoreElement) && scoreElement.TryGetDouble(out var s)
                ? s
                : 0d;
            list.Add(new VectorSearchResult(chunkId, score));
        }

        return list;
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

            var client = CreateClient();
            using var get = await client.GetAsync($"/collections/{_options.CollectionName}", cancellationToken);
            if (!get.IsSuccessStatusCode)
            {
                var body = new
                {
                    vectors = new
                    {
                        size = _options.VectorSize,
                        distance = _options.Distance,
                    },
                };

                using var create = await client.PutAsJsonAsync($"/collections/{_options.CollectionName}", body, cancellationToken);
                if (!create.IsSuccessStatusCode)
                {
                    var content = await create.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("Qdrant create collection failed: {Status} {Body}", create.StatusCode, content);
                }
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private List<object> BuildFilter(Guid? collectionId)
    {
        var must = new List<object>
        {
            new { key = "tenantId", match = new { value = userContext.TenantId.ToString() } },
        };

        if (collectionId.HasValue)
        {
            must.Add(new { key = "collectionId", match = new { value = collectionId.Value.ToString() } });
        }

        return must;
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.Url.TrimEnd('/'));
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            client.DefaultRequestHeaders.Remove("api-key");
            client.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        }
        return client;
    }
}
