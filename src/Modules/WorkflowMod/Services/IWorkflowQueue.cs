namespace WorkflowMod.Services;

public interface IWorkflowQueue
{
    ValueTask EnqueueAsync(WorkflowTask task);
    ValueTask<WorkflowTask> DequeueAsync(CancellationToken cancellationToken);
}
