namespace Share.Services;

public interface IMcpToolExecutorFacade
{
    Task<ToolExecutionResult> ExecuteAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default);
}
