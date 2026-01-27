namespace CoreMod.Models;

/// <summary>
/// 模型消息
/// </summary>
public sealed class ModelMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 工具调用
/// </summary>
public sealed class ToolCall
{
    public required string Name { get; set; }

    public string ArgumentsJson { get; set; } = string.Empty;
}
