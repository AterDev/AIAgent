namespace KnowledgeBaseMod.Services;

/// <summary>
/// 向量存储占位实现
/// </summary>
public class NullVectorStore : IVectorStore
{
    public Task<Dictionary<Guid, string>> UpsertAsync(RagDocument document, IReadOnlyList<RagChunk> chunks, CancellationToken cancellationToken = default)
    {
        var result = chunks.ToDictionary(c => c.Id, _ => Guid.NewGuid().ToString("N"));
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Guid? collectionId, string query, int topK, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VectorSearchResult> result = [];
        return Task.FromResult(result);
    }
}
