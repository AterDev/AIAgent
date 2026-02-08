namespace CoreMod.Services;

using CoreMod.Models;

public sealed class ModelInvokeRequest
{
    public required string Model { get; set; }

    public string? Provider { get; set; }

    public string? Scene { get; set; }

    public List<ModelInvokeMessage> Messages { get; set; } = [];

    /// <summary>
    /// Tool definitions for function calling
    /// </summary>
    public List<ModelToolDefinition> ToolDefinitions { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = new();
}

public sealed class ModelInvokeMessage
{
    public required string Role { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tool call ID (for tool result messages)
    /// </summary>
    public string? ToolCallId { get; set; }
}

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

