namespace CoreMod.Models;

/// <summary>
/// 模型消息
/// </summary>
public sealed class ModelMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tool call ID (for tool result messages)
    /// </summary>
    public string? ToolCallId { get; set; }

    /// <summary>
    /// 多模态附件（图片等），可为空。
    /// </summary>
    public List<ModelAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// 工具调用
/// </summary>
public sealed class ToolCall
{
    /// <summary>
    /// Tool call ID (from model response)
    /// </summary>
    public string? Id { get; set; }

    public required string Name { get; set; }

    public string ArgumentsJson { get; set; } = string.Empty;
}

/// <summary>
/// 工具定义（传递给模型的 function schema）
/// </summary>
public sealed class ModelToolDefinition
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// JSON Schema for parameters
    /// </summary>
    public string? ParametersJson { get; set; }
}
