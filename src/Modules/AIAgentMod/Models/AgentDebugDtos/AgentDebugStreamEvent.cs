namespace AIAgentMod.Models.AgentDebugDtos;

public sealed class AgentDebugStreamEvent
{
    public string Type { get; set; } = "message";

    public string? RequestId { get; set; }

    public AgentDebugMessage? Message { get; set; }

    public AgentDebugToolCall? ToolCall { get; set; }

    public AgentDebugMetrics? Metrics { get; set; }

    public string? Error { get; set; }
}

public sealed class AgentDebugMessage
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AgentDebugToolCall
{
    public string Name { get; set; } = string.Empty;

    public object? Input { get; set; }

    public object? Output { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AgentDebugMetrics
{
    public int DurationMs { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public int ToolCallCount { get; set; }
}
