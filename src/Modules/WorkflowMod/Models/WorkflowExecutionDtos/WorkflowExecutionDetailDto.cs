namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 工作流执行 DetailDto
/// </summary>
/// <see cref="WorkflowExecution"/>
public class WorkflowExecutionDetailDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(4000)]
    public string? InputJson { get; set; }

    [MaxLength(4000)]
    public string? OutputJson { get; set; }

    public WorkflowExecutionStatus Status { get; set; }

    public DateTimeOffset? CompletedTime { get; set; }

    public int DurationMs { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
