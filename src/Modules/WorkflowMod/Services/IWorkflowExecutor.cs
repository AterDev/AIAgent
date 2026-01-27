namespace WorkflowMod.Services;

public interface IWorkflowExecutor
{
    Task<bool> ExecuteAsync(Guid executionId, CancellationToken cancellationToken = default);
}
