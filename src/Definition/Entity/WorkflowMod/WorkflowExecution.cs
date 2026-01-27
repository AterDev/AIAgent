namespace Entity.WorkflowMod;

/// <summary>
/// 工作流执行记录
/// </summary>
[Index(nameof(WorkflowId), nameof(Status))]
public class WorkflowExecution : EntityBase
{
    public Guid WorkflowId { get; set; }

    [ForeignKey(nameof(WorkflowId))]
    public Workflow? Workflow { get; set; }

    public WorkflowExecutionStatus Status { get; set; }

    [MaxLength(4000)]
    public string InputJson { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string OutputJson { get; set; } = string.Empty;

    public DateTimeOffset? CompletedTime { get; set; }

    public int DurationMs { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
