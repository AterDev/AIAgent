using Entity.KnowledgeBaseMod;
namespace KnowledgeBaseMod.Models.RagCollectionDtos;

/// <summary>
/// 知识库 FilterDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagCollection"/>
public class RagCollectionFilterDto : FilterBase
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsEnabled { get; set; }
}
