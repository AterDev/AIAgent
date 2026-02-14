namespace KnowledgeBaseMod.Models.RagChunkDtos;

/// <summary>
/// 分块 FilterDto
/// </summary>
/// <see cref="RagChunk"/>
public class RagChunkFilterDto : FilterBase
{
    public Guid? DocumentId { get; set; }
}
