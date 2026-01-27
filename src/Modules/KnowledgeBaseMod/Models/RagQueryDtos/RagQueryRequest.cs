namespace KnowledgeBaseMod.Models.RagQueryDtos;

/// <summary>
/// 知识库检索请求
/// </summary>
public class RagQueryRequest
{
    public required string Query { get; set; }

    public Guid? CollectionId { get; set; }

    public int TopK { get; set; } = 5;
}
