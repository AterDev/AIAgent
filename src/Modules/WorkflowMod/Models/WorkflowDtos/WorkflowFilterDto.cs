namespace WorkflowMod.Models.WorkflowDtos;

/// <summary>
/// 工作流 FilterDto
/// </summary>
/// <see cref="Entity.WorkflowMod.Workflow"/>
public class WorkflowFilterDto : FilterBase
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public bool? IsPublished { get; set; }
}
