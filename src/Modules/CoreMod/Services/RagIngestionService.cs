using Share.Models;
using Share.Services;
using Entity.KnowledgeBaseMod;

namespace CoreMod.Services;

/// <summary>
/// RAG 文档解析与向量化流程
/// 负责文档的解析、分块、向量化和存储
/// </summary>
public class RagIngestionService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IDocumentParser parser,
    ITextChunker chunker,
    IVectorStore vectorStore,
    ILogger<RagIngestionService> logger
)
{
    public async Task<bool> IngestAsync(Guid documentId, string? contentText = null, CancellationToken cancellationToken = default)
    {
        return await IngestAsync(documentId, userContext.TenantId, contentText, cancellationToken);
    }

    public async Task<bool> IngestAsync(Guid documentId, Guid tenantId, string? contentText = null, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var document = await dbContext.RagDocuments
            .FirstOrDefaultAsync(q => q.Id == documentId && q.TenantId == tenantId, cancellationToken);

        if (document is null)
        {
            return false;
        }

        document.Status = RagDocumentStatus.Parsing;
        document.ErrorMessage = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        DocumentParseResult parseResult;
        try
        {
            parseResult = await parser.ParseAsync(document, contentText, cancellationToken);
        }
        catch (Exception ex)
        {
            document.Status = RagDocumentStatus.Failed;
            document.ErrorMessage = ex.Message;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogWarning(ex, "Parse failed for document {DocumentId}", documentId);
            return false;
        }

        document.Status = RagDocumentStatus.Vectorizing;
        await dbContext.SaveChangesAsync(cancellationToken);

        var chunks = chunker.Split(parseResult.Text);
        var chunkEntities = chunks.Select(c => new RagChunk
        {
            DocumentId = document.Id,
            ChunkIndex = c.Index,
            Content = c.Content,
            TokenCount = c.TokenCount,
        }).ToList();

        var existing = dbContext.RagChunks.Where(q => q.DocumentId == document.Id);
        dbContext.RagChunks.RemoveRange(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.RagChunks.AddRangeAsync(chunkEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var vectorMap = await vectorStore.UpsertAsync(document, chunkEntities, cancellationToken);
        foreach (var chunk in chunkEntities)
        {
            if (vectorMap.TryGetValue(chunk.Id, out var vectorId))
            {
                chunk.VectorId = vectorId;
            }
        }

        document.ChunkCount = chunkEntities.Count;
        document.TokenCount = parseResult.TokenCount;
        document.Status = RagDocumentStatus.Completed;
        document.ErrorMessage = null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
