namespace WorkflowMod.Services;

public class WorkflowQueue(IEntityTaskQueue<WorkflowTask> queue)
{
    public ValueTask EnqueueAsync(WorkflowTask task)
    {
        return queue.AddItemAsync(task);
    }

    public ValueTask<WorkflowTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.DequeueAsync(cancellationToken);
    }
}
