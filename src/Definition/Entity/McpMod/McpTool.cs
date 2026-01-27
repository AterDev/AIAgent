namespace Entity.McpMod;

/// <summary>
/// MCP 工具定义
/// </summary>
[Index(nameof(Name), nameof(Version), IsUnique = true)]
public class McpTool : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public McpToolType ToolType { get; set; }

    [MaxLength(40)]
    public string Version { get; set; } = "1.0";

    public bool IsEnabled { get; set; } = true;

    [MaxLength(4000)]
    public string? SchemaJson { get; set; }

    public Guid? ServerId { get; set; }
}
