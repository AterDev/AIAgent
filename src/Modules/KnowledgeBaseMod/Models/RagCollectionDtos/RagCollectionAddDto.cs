using Entity.KnowledgeBaseMod;
namespace KnowledgeBaseMod.Models.RagCollectionDtos;

/// <summary>
/// 知识库 AddDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagCollection"/>
public class RagCollectionAddDto
{
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; } = true;

    public List<string>? Tags { get; set; }
}
