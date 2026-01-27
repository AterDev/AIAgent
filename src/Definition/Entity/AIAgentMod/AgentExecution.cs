namespace Entity.AIAgentMod;

using System.ComponentModel;

public enum AgentExecutionStatus
{
    [Description("Running")]
    Running = 0,

    [Description("Completed")]
    Completed = 1,

    [Description("Failed")]
    Failed = 2,

    [Description("Canceled")]
    Canceled = 3,
}

/// <summary>
/// Agent 执行记录
/// </summary>
[Index(nameof(AgentId), nameof(Status))]
public class AgentExecution : EntityBase
{
    public Guid AgentId { get; set; }

    [ForeignKey(nameof(AgentId))]
    public AIAgent? Agent { get; set; }

    public AgentExecutionStatus Status { get; set; }

    [MaxLength(4000)]
    public string InputJson { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string OutputJson { get; set; } = string.Empty;

    public DateTimeOffset? CompletedTime { get; set; }

    public int DurationMs { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
