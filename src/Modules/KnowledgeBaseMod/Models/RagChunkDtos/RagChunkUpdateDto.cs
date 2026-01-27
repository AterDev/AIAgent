namespace KnowledgeBaseMod.Models.RagChunkDtos;

/// <summary>
/// 分块 UpdateDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagChunk"/>
public class RagChunkUpdateDto
{
    public int? ChunkIndex { get; set; }

    [MaxLength(4000)]
    public string? Content { get; set; }

    public int? TokenCount { get; set; }

    [MaxLength(100)]
    public string? VectorId { get; set; }
}
