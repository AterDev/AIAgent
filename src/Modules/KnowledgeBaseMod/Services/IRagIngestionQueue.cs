namespace KnowledgeBaseMod.Services;

public interface IRagIngestionQueue
{
    ValueTask EnqueueAsync(RagIngestionTask task);
    ValueTask<RagIngestionTask> DequeueAsync(CancellationToken cancellationToken);
}
