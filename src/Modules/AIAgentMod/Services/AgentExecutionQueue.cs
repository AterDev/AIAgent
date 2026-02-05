namespace AIAgentMod.Services;

public class AgentExecutionQueue(IEntityTaskQueue<AgentExecutionTask> queue)
{
    public ValueTask EnqueueAsync(AgentExecutionTask task)
    {
        return queue.AddItemAsync(task);
    }

    public ValueTask<AgentExecutionTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.DequeueAsync(cancellationToken);
    }
}
