namespace KnowledgeBaseMod.Models.RagChunkDtos;

/// <summary>
/// 分块 AddDto
/// </summary>
/// <see cref="RagChunk"/>
public class RagChunkAddDto
{
    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    [MaxLength(4000)]
    public string Content { get; set; } = default!;

    public int TokenCount { get; set; }

    [MaxLength(100)]
    public string? VectorId { get; set; }
}
