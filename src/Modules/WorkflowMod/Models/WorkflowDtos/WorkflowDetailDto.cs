namespace WorkflowMod.Models.WorkflowDtos;

/// <summary>
/// 工作流 DetailDto
/// </summary>
/// <see cref="Entity.WorkflowMod.Workflow"/>
public class WorkflowDetailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(8000)]
    public string? DefinitionJson { get; set; }

    public int Version { get; set; }

    public bool IsPublished { get; set; }
}
