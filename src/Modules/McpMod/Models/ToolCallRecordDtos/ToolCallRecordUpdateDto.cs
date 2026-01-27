namespace McpMod.Models.ToolCallRecordDtos;

/// <summary>
/// 工具调用记录 UpdateDto
/// </summary>
/// <see cref="Entity.McpMod.ToolCallRecord"/>
public class ToolCallRecordUpdateDto
{
    [MaxLength(4000)]
    public string? OutputJson { get; set; }

    public int? DurationMs { get; set; }

    public ToolCallStatus? Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
