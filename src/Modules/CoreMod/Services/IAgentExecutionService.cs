namespace CoreMod.Services;

/// <summary>
/// Agent 执行服务接口 - CoreMod 定义，由 AIAgentMod 实现
/// </summary>
public interface IAgentExecutionService
{
    /// <summary>
    /// 执行 Agent
    /// </summary>
    /// <param name="executionId">执行ID</param>
    /// <param name="applicationId">应用ID</param>
    /// <param name="inputJson">输入 JSON</param>
    /// <param name="cancellationToken"></param>
    /// <returns>执行是否成功</returns>
    Task<bool> ExecuteAsync(Guid executionId, Guid applicationId, string? inputJson, CancellationToken cancellationToken = default);
}

