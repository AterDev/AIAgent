namespace McpMod.Models.ToolCallRecordDtos;

/// <summary>
/// 工具调用记录 ItemDto
/// </summary>
/// <see cref="Entity.McpMod.ToolCallRecord"/>
public class ToolCallRecordItemDto
{
    public Guid Id { get; set; }
    public Guid ToolId { get; set; }
    public ToolCallStatus Status { get; set; }
    public int DurationMs { get; set; }
}
