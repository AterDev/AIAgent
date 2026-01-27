using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 工作流执行 UpdateDto
/// </summary>
/// <see cref="Entity.WorkflowMod.WorkflowExecution"/>
public class WorkflowExecutionUpdateDto
{
    [MaxLength(4000)]
    public string? OutputJson { get; set; }

    public DateTimeOffset? CompletedTime { get; set; }

    public int? DurationMs { get; set; }

    public WorkflowExecutionStatus? Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
