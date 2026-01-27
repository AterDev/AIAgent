namespace McpMod.Models.McpToolDtos;

/// <summary>
/// MCP 工具 AddDto
/// </summary>
/// <see cref="Entity.McpMod.McpTool"/>
public class McpToolAddDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public McpToolType ToolType { get; set; }

    [MaxLength(40)]
    public string? Version { get; set; }

    public bool IsEnabled { get; set; } = true;

    [MaxLength(4000)]
    public string? SchemaJson { get; set; }

    public Guid? ServerId { get; set; }
}
