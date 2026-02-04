namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 文档
/// </summary>
[Index(nameof(CollectionId), nameof(Name))]
public class RagDocument : EntityBase
{
    public Guid CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public RagCollection? Collection { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型（后缀，如 pdf、docx、txt 等）
    /// </summary>
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// 存储服务商ID
    /// </summary>
    public Guid StorageProviderId { get; set; }

    public RagDocumentStatus Status { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<string> Roles { get; set; } = [];

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public int ChunkCount { get; set; }

    public int TokenCount { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
