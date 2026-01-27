using Entity.KnowledgeBaseMod;
namespace KnowledgeBaseMod.Models.RagChunkDtos;

/// <summary>
/// 分块 DetailDto
/// </summary>
/// <see cref="Entity.KnowledgeBaseMod.RagChunk"/>
public class RagChunkDetailDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    public int ChunkIndex { get; set; }

    [MaxLength(4000)]
    public string? Content { get; set; }

    public int TokenCount { get; set; }

    [MaxLength(100)]
    public string? VectorId { get; set; }
}
