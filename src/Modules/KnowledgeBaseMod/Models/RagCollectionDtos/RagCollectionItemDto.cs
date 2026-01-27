namespace KnowledgeBaseMod.Models.RagCollectionDtos;

/// <summary>
/// 知识库 ItemDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagCollection"/>
public class RagCollectionItemDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public bool IsPublic { get; set; }

    public bool IsEnabled { get; set; }
}
