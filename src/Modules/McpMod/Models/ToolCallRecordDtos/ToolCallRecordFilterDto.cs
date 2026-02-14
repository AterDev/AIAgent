namespace McpMod.Models.ToolCallRecordDtos;

/// <summary>
/// 工具调用记录 FilterDto
/// </summary>
/// <see cref="ToolCallRecord"/>
public class ToolCallRecordFilterDto : FilterBase
{
    public Guid? ToolId { get; set; }

    public ToolCallStatus? Status { get; set; }
}
