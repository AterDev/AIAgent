namespace AIAgentMod.Services;

public interface IAgentExecutionQueue
{
    ValueTask EnqueueAsync(AgentExecutionTask task);
    ValueTask<AgentExecutionTask> DequeueAsync(CancellationToken cancellationToken);
}
