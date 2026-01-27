namespace McpMod.Models.ToolExecutionDtos;

/// <summary>
/// 工具执行结果
/// </summary>
public class ToolExecutionResult
{
    public bool Success { get; set; }

    public string? OutputJson { get; set; }

    public string? ErrorMessage { get; set; }
}
