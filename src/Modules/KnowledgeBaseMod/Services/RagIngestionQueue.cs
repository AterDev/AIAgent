namespace KnowledgeBaseMod.Services;

public class RagIngestionQueue(IEntityTaskQueue<RagIngestionTask> queue) : IRagIngestionQueue
{
    public ValueTask EnqueueAsync(RagIngestionTask task)
    {
        return queue.AddItemAsync(task);
    }

    public ValueTask<RagIngestionTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.DequeueAsync(cancellationToken);
    }
}
