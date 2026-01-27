namespace Entity.WorkflowMod;

/// <summary>
/// 工作流定义
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class Workflow : EntityBase
{
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string DefinitionJson { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public bool IsPublished { get; set; }
}
