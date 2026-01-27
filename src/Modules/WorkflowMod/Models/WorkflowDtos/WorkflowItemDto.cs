using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowDtos;

/// <summary>
/// 工作流 ItemDto
/// </summary>
/// <see cref="Entity.WorkflowMod.Workflow"/>
public class WorkflowItemDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public int Version { get; set; }

    public bool IsPublished { get; set; }
}
