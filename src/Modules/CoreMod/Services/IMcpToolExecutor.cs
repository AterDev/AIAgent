using Share.Services;

namespace CoreMod.Services;

/// <summary>
/// MCP 工具执行接口 - CoreMod 定义，由 McpMod 实现
/// </summary>
public interface IMcpToolExecutor
{
    /// <summary>
    /// 执行 MCP 工具
    /// </summary>
    /// <param name="request">工具执行请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>执行结果</returns>
    Task<ToolExecutionResult> ExecuteAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default);
}
