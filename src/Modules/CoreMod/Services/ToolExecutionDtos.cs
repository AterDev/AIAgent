namespace CoreMod.Services;

public sealed class ToolExecutionRequest
{
    public required string ToolName { get; set; }

    public string? ArgumentsJson { get; set; }

    public Guid? ApplicationId { get; set; }

    public Guid? AgentId { get; set; }
}

public sealed class ToolExecutionResult
{
    public bool Success { get; set; }

    public string? OutputJson { get; set; }

    public string? ErrorMessage { get; set; }
}

