namespace CoreMod.Models.ModelInvoke;

public sealed class ModelInvokeMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Assistant tool calls that must be preserved in history before tool results.
    /// </summary>
    public List<ToolCall> ToolCalls { get; set; } = [];

    /// <summary>
    /// Tool call ID (for tool result messages)
    /// </summary>
    public string? ToolCallId { get; set; }

    /// <summary>
    /// 多模态附件（图片等），可为空。
    /// </summary>
    public List<ModelAttachment> Attachments { get; set; } = new();
}
