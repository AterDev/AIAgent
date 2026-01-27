using Entity.WorkflowMod;
namespace WorkflowMod.Models.WorkflowDtos;

/// <summary>
/// 工作流 AddDto
/// </summary>
/// <see cref="Entity.WorkflowMod.Workflow"/>
public class WorkflowAddDto
{
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(8000)]
    public string DefinitionJson { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public bool IsPublished { get; set; }
}
