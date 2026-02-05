namespace CoreMod.Services;

public class RagIngestionQueue(IEntityTaskQueue<RagIngestionTask> queue)
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
