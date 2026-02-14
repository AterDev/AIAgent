namespace CoreMod.Models.ModelInvoke;

public sealed class ModelInvokeResponse
{
    public bool Success { get; set; }

    public string? Content { get; set; }

    /// <summary>
    /// Tool calls returned by the model (structured function calling)
    /// </summary>
    public List<ToolCall> ToolCalls { get; set; } = [];

    public UsageStats Usage { get; set; } = new();

    public string? ErrorMessage { get; set; }
}