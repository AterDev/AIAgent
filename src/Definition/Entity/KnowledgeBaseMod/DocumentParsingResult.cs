namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 文档解析结果
/// </summary>
public class DocumentParsingResult : EntityBase
{
    /// <summary>
    /// Rag 文档 ID
    /// </summary>
    public Guid RagDocumentId { get; set; }

    /// <summary>
    /// Rag 文档
    /// </summary>
    public required RagDocument RagDocument { get; set; }

    /// <summary>
    /// 文档格式类型
    /// </summary>
    public DocumentFormatType DocumentFormat { get; set; }

    /// <summary>
    /// 文档解析状态
    /// </summary>
    public DocumentParsingStatus ParsingStatus { get; set; }

    /// <summary>
    /// 文本内容（解析后）
    /// </summary>
    public required string TextContent { get; set; }

    /// <summary>
    /// 页数（对于 PDF、Word 等多页文档）
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// 字数统计
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// 错误或警告信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 解析开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 解析完成时间
    /// </summary>
    public DateTime? CompletedTime { get; set; }

    /// <summary>
    /// 解析耗时（毫秒）
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// 解析版本
    /// </summary>
    public int ParsingVersion { get; set; } = 1;
}