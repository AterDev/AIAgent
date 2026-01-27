namespace Entity.McpMod;

/// <summary>
/// MCP 工具调用记录
/// </summary>
[Index(nameof(ToolId), nameof(Status))]
public class ToolCallRecord : EntityBase
{
    public Guid ToolId { get; set; }

    [ForeignKey(nameof(ToolId))]
    public McpTool? Tool { get; set; }

    public Guid? ApplicationId { get; set; }

    public Guid? AgentId { get; set; }

    [MaxLength(4000)]
    public string InputJson { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string OutputJson { get; set; } = string.Empty;

    public int DurationMs { get; set; }

    public ToolCallStatus Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
