namespace KnowledgeBaseMod.Models.RagQueryDtos;

/// <summary>
/// 知识库检索结果
/// </summary>
public class RagQueryResult
{
    public List<RagQueryItem> Items { get; set; } = new();
}

public class RagQueryItem
{
    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Score { get; set; }
}
