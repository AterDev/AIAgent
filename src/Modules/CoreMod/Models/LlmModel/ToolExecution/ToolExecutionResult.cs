namespace CoreMod.Models.ToolExecution;

public sealed class ToolExecutionResult
{
    public bool Success { get; set; }

    public string? OutputJson { get; set; }

    public string? ErrorMessage { get; set; }
}