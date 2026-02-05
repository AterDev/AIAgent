namespace Share.Services;

public interface IVectorStore
{
    Task<Dictionary<Guid, string>> UpsertAsync(RagDocument document, IReadOnlyList<RagChunk> chunks, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Guid? collectionId, string query, int topK, CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task DeleteChunksAsync(IEnumerable<Guid> chunkIds, CancellationToken cancellationToken = default);
}

public record VectorSearchResult(Guid ChunkId, double Score);
