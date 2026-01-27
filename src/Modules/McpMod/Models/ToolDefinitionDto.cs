namespace McpMod.Models;

/// <summary>
/// 工具定义（对外）
/// </summary>
public class ToolDefinitionDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? SchemaJson { get; set; }

    public string Version { get; set; } = "1.0";

    public McpToolType ToolType { get; set; }
}
