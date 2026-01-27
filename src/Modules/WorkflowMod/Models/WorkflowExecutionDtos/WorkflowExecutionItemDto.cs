using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 工作流执行 ItemDto
/// </summary>
/// <see cref="Entity.WorkflowMod.WorkflowExecution"/>
public class WorkflowExecutionItemDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public WorkflowExecutionStatus Status { get; set; }
    public int DurationMs { get; set; }
}
