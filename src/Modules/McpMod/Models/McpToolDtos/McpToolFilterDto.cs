using Entity.McpMod;
namespace McpMod.Models.McpToolDtos;

/// <summary>
/// MCP 工具 FilterDto
/// </summary>
/// <see cref="Entity.McpMod.McpTool"/>
public class McpToolFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public McpToolType? ToolType { get; set; }

    public bool? IsEnabled { get; set; }
}
