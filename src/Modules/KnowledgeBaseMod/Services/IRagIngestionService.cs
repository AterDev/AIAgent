namespace KnowledgeBaseMod.Services;

public interface IRagIngestionService
{
    Task<bool> IngestAsync(Guid documentId, string? contentText = null, CancellationToken cancellationToken = default);
}
