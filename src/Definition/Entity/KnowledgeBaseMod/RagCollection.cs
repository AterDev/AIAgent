namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 知识库/文档集
/// </summary>
[Index(nameof(Name))]
public class RagCollection : EntityBase
{
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; } = true;

    public List<string> Tags { get; set; } = [];
}
