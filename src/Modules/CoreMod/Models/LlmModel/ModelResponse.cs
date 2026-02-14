namespace CoreMod.Models;

/// <summary>
/// 模型调用响应
/// </summary>
public sealed class ModelResponse
{
    public bool Success { get; set; }

    public string? Content { get; set; }

    public List<ToolCall> ToolCalls { get; set; } = new();

    public UsageStats Usage { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Token 使用情况
/// </summary>
public sealed class UsageStats
{
    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }
}
