using McpMod.Models.ToolExecutionDtos;

namespace McpMod.Services;

/// <summary>
/// MCP 工具执行入口
/// </summary>
public interface IMcpToolExecutor
{
    Task<ToolExecutionResult> ExecuteAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default);
}
