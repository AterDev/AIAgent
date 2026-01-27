using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 工作流执行 FilterDto
/// </summary>
/// <see cref="Entity.WorkflowMod.WorkflowExecution"/>
public class WorkflowExecutionFilterDto : FilterBase
{
    public Guid? WorkflowId { get; set; }
    public WorkflowExecutionStatus? Status { get; set; }
}
