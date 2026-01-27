namespace McpMod.Models.ToolExecutionDtos;

/// <summary>
/// 工具执行请求
/// </summary>
public class ToolExecutionRequest
{
    public required string ToolName { get; set; }

    public string? ArgumentsJson { get; set; }

    public Guid? ApplicationId { get; set; }

    public Guid? AgentId { get; set; }
}
