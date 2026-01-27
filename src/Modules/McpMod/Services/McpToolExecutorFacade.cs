using McpMod.Models.ToolExecutionDtos;

namespace McpMod.Services;

/// <summary>
/// Share 接口适配器（MCP 工具执行）
/// </summary>
public class McpToolExecutorFacade(IMcpToolExecutor executor) : Share.Services.IMcpToolExecutorFacade
{
    public async Task<Share.Services.ToolExecutionResult> ExecuteAsync(Share.Services.ToolExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await executor.ExecuteAsync(new ToolExecutionRequest
        {
            ToolName = request.ToolName,
            ArgumentsJson = request.ArgumentsJson,
            ApplicationId = request.ApplicationId,
            AgentId = request.AgentId,
        }, cancellationToken);

        return new Share.Services.ToolExecutionResult
        {
            Success = result.Success,
            OutputJson = result.OutputJson,
            ErrorMessage = result.ErrorMessage,
        };
    }
}
