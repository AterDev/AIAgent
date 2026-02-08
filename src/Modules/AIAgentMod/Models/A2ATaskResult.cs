namespace AIAgentMod.Models;

/// <summary>
/// A2A task execution result.
/// </summary>
public sealed class A2ATaskResult
{
    public bool Success { get; set; }
    public string? TaskId { get; set; }
    public string? ContextId { get; set; }
    public string? Status { get; set; }
    public string? Content { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
}
