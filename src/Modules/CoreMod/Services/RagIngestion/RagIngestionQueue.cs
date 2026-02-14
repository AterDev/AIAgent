namespace CoreMod.Services.RagIngestion;

public class RagIngestionQueue(IEntityTaskQueue<RagDocumentIngestionTask> queue)
{
    public ValueTask EnqueueAsync(RagDocumentIngestionTask task)
    {
        return queue.AddItemAsync(task);
    }

    public ValueTask<RagDocumentIngestionTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.DequeueAsync(cancellationToken);
    }
}