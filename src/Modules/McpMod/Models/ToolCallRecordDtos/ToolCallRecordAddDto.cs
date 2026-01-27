namespace McpMod.Models.ToolCallRecordDtos;

/// <summary>
/// 工具调用记录 AddDto
/// </summary>
/// <see cref="Entity.McpMod.ToolCallRecord"/>
public class ToolCallRecordAddDto
{
    public Guid ToolId { get; set; }

    public Guid? ApplicationId { get; set; }

    public Guid? AgentId { get; set; }

    [MaxLength(4000)]
    public string? InputJson { get; set; }

    [MaxLength(4000)]
    public string? OutputJson { get; set; }

    public int DurationMs { get; set; }

    public ToolCallStatus Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
