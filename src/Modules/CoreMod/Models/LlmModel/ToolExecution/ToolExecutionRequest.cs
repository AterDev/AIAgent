namespace CoreMod.Models.ToolExecution;

public sealed class ToolExecutionRequest
{
    public required string ToolName { get; set; }

    public string? ArgumentsJson { get; set; }

    public Guid? ApplicationId { get; set; }

    public Guid? AgentId { get; set; }
}
