namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档 ItemDto
/// </summary>
/// <see cref="RagDocument"/>
public class RagDocumentItemDto
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public RagDocumentStatus Status { get; set; }

    public int ChunkCount { get; set; }
}
