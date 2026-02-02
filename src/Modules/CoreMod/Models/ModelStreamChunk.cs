namespace CoreMod.Models;

/// <summary>
/// 模型流式输出片段
/// </summary>
public sealed class ModelStreamChunk
{
    public string? Delta { get; set; }

    public UsageStats? Usage { get; set; }

    public bool IsFinal { get; set; }

    public string? ErrorMessage { get; set; }
}
