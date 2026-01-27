using Entity.McpMod;
namespace McpMod.Models.McpToolDtos;

/// <summary>
/// MCP 工具 ItemDto
/// </summary>
/// <see cref="Entity.McpMod.McpTool"/>
public class McpToolItemDto
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    public McpToolType ToolType { get; set; }

    public bool IsEnabled { get; set; }
}
