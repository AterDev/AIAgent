namespace KnowledgeBaseMod.Models.RagCollectionDtos;

/// <summary>
/// 知识库 UpdateDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagCollection"/>
public class RagCollectionUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsEnabled { get; set; }

    public List<string>? Tags { get; set; }
}
