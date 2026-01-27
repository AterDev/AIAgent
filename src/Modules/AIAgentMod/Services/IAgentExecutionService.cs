namespace AIAgentMod.Services;

public interface IAgentExecutionService
{
    Task<bool> ExecuteAsync(Guid executionId, Guid applicationId, string? inputJson, CancellationToken cancellationToken = default);
}
