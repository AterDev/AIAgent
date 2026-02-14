namespace CoreMod.Models.ModelInvoke;

public sealed class ModelInvokeMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tool call ID (for tool result messages)
    /// </summary>
    public string? ToolCallId { get; set; }
}
