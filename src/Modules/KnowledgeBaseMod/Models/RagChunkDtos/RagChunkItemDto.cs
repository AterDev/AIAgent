namespace KnowledgeBaseMod.Models.RagChunkDtos;

/// <summary>
/// 分块 ItemDto
/// </summary>
/// <see cref="RagChunk"/>
public class RagChunkItemDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public int TokenCount { get; set; }
}
