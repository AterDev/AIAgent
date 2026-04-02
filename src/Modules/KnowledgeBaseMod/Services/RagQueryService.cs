using Perigon.AspNetCore.Constants;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 知识库检索服务（基础实现）
/// </summary>
public class RagQueryService(
    TenantDbFactory dbContextFactory,
    ILogger<RagQueryService> logger,
    IUserContext userContext,
    IVectorStore vectorStore
) : IRagQueryService
{
    public async Task<RagQueryResult> QueryAsync(RagQueryRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return new RagQueryResult();
        }

        logger.LogDebug("RAG query: {Query}", request.Query);

        var topK = request.TopK <= 0 ? 5 : request.TopK;

        var keywordItems = await QueryKeywordAsync(dbContext, request, topK, cancellationToken);
        var vectorItems = await QueryVectorAsync(dbContext, request, topK, cancellationToken);

        var combined = CombineScores(keywordItems, vectorItems, topK);
        return new RagQueryResult { Items = combined };
    }

    private async Task<List<RagQueryItem>> QueryKeywordAsync(
        DefaultDbContext dbContext,
        RagQueryRequest request,
        int topK,
        CancellationToken cancellationToken)
    {
        if (request.CollectionId.HasValue)
        {
            var query = dbContext.RagChunks
                .AsNoTracking()
                .Where(c => c.TenantId == userContext.TenantId
                    && c.Document != null
                    && c.Document.TenantId == userContext.TenantId
                    && (!_isApplicationRequest() || dbContext.ApplicationRagCollectionPermissions.Any(link =>
                        link.TenantId == userContext.TenantId
                        && link.IsEnabled
                        && link.ApplicationId == userContext.UserId
                        && link.RagCollectionId == c.Document.CollectionId))
                    && c.Document.CollectionId == request.CollectionId
                    && c.Content.Contains(request.Query))
                .Select(c => new RagQueryItem
                {
                    DocumentId = c.DocumentId,
                    Content = c.Content,
                    Score = 1.0,
                });

            return await query.Take(topK).ToListAsync(cancellationToken);
        }

        var queryAll = dbContext.RagChunks
            .AsNoTracking()
            .Where(q => q.TenantId == userContext.TenantId
                && q.Content.Contains(request.Query)
                && (!_isApplicationRequest() || (q.Document != null && dbContext.ApplicationRagCollectionPermissions.Any(link =>
                    link.TenantId == userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == userContext.UserId
                    && link.RagCollectionId == q.Document.CollectionId))))
            .Select(chunk => new RagQueryItem
            {
                DocumentId = chunk.DocumentId,
                Content = chunk.Content,
                Score = 1.0,
            });

        return await queryAll.Take(topK).ToListAsync(cancellationToken);
    }

    private async Task<List<RagQueryItem>> QueryVectorAsync(
        DefaultDbContext dbContext,
        RagQueryRequest request,
        int topK,
        CancellationToken cancellationToken)
    {
        if (_isApplicationRequest())
        {
            if (request.CollectionId.HasValue)
            {
                var hasAccess = await dbContext.ApplicationRagCollectionPermissions
                    .AsNoTracking()
                    .AnyAsync(link => link.TenantId == userContext.TenantId
                        && link.IsEnabled
                        && link.ApplicationId == userContext.UserId
                        && link.RagCollectionId == request.CollectionId.Value, cancellationToken);

                if (!hasAccess)
                {
                    return [];
                }
            }
            else
            {
                var hasAnyAccessibleCollection = await dbContext.ApplicationRagCollectionPermissions
                    .AsNoTracking()
                    .AnyAsync(link => link.TenantId == userContext.TenantId
                        && link.IsEnabled
                        && link.ApplicationId == userContext.UserId, cancellationToken);

                if (!hasAnyAccessibleCollection)
                {
                    return [];
                }
            }
        }

        var vectorHits = await vectorStore.SearchAsync(request.CollectionId, request.Query, topK, cancellationToken);
        if (vectorHits.Count == 0)
        {
            return [];
        }

        var chunkIds = vectorHits.Select(v => v.ChunkId).ToList();
        var chunkMap = await dbContext.RagChunks
            .AsNoTracking()
            .Where(q => q.TenantId == userContext.TenantId
                && chunkIds.Contains(q.Id)
                && (!_isApplicationRequest() || (q.Document != null && dbContext.ApplicationRagCollectionPermissions.Any(link =>
                    link.TenantId == userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == userContext.UserId
                    && link.RagCollectionId == q.Document.CollectionId))))
            .Select(c => new { c.Id, c.DocumentId, c.Content })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var items = new List<RagQueryItem>();
        foreach (var hit in vectorHits)
        {
            if (chunkMap.TryGetValue(hit.ChunkId, out var chunk))
            {
                items.Add(new RagQueryItem
                {
                    DocumentId = chunk.DocumentId,
                    Content = chunk.Content,
                    Score = hit.Score,
                });
            }
        }

        return items;
    }

    private static List<RagQueryItem> CombineScores(
        List<RagQueryItem> keywordItems,
        List<RagQueryItem> vectorItems,
        int topK)
    {
        const double vectorWeight = 0.7;
        const double keywordWeight = 0.3;

        var result = new Dictionary<string, RagQueryItem>();
        foreach (var item in keywordItems)
        {
            var key = item.DocumentId + "|" + item.Content;
            result[key] = new RagQueryItem
            {
                DocumentId = item.DocumentId,
                Content = item.Content,
                Score = keywordWeight * item.Score,
            };
        }

        foreach (var item in vectorItems)
        {
            var key = item.DocumentId + "|" + item.Content;
            if (result.TryGetValue(key, out var existing))
            {
                existing.Score += vectorWeight * item.Score;
            }
            else
            {
                result[key] = new RagQueryItem
                {
                    DocumentId = item.DocumentId,
                    Content = item.Content,
                    Score = vectorWeight * item.Score,
                };
            }
        }

        return result.Values
            .OrderByDescending(q => q.Score)
            .Take(topK)
            .ToList();
    }

    bool _isApplicationRequest()
    {
        return userContext.IsRole(WebConst.Application) && userContext.UserId != Guid.Empty;
    }
}
