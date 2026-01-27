using Entity.KnowledgeBaseMod;
namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档 FilterDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagDocument"/>
public class RagDocumentFilterDto : FilterBase
{
    public Guid? CollectionId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public RagDocumentStatus? Status { get; set; }
}
