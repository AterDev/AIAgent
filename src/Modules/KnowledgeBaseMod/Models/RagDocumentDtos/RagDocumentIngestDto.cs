namespace KnowledgeBaseMod.Models.RagDocumentDtos;

/// <summary>
/// 文档解析/向量化输入
/// </summary>
public class RagDocumentIngestDto
{
    /// <summary>
    /// 直接提供文本内容（可选）
    /// </summary>
    public string? ContentText { get; set; }
}
