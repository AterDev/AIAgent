namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 文档分块
/// </summary>
[Index(nameof(DocumentId), nameof(ChunkIndex), IsUnique = true)]
public class RagChunk : EntityBase
{
    public Guid DocumentId { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public RagDocument? Document { get; set; }

    public int ChunkIndex { get; set; }

    [MaxLength(4000)]
    public required string Content { get; set; }

    public int TokenCount { get; set; }

    [MaxLength(100)]
    public string? VectorId { get; set; }
}
