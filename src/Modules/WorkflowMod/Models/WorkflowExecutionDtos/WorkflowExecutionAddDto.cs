using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 工作流执行 AddDto
/// </summary>
/// <see cref="Entity.WorkflowMod.WorkflowExecution"/>
public class WorkflowExecutionAddDto
{
    public Guid WorkflowId { get; set; }

    [MaxLength(4000)]
    public string? InputJson { get; set; }

    public WorkflowExecutionStatus Status { get; set; }
}
