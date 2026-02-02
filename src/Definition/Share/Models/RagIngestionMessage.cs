using Perigon.AspNetCore.Options;

namespace Share.Models;

/// <summary>
/// RAG 文档处理消息
/// </summary>
public class RagIngestionMessage
{
    /// <summary>
    /// 文档 ID
    /// </summary>
    public required Guid DocumentId { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public required Guid TenantId { get; set; }

    /// <summary>
    /// 知识库集合 ID
    /// </summary>
    public required Guid CollectionId { get; set; }

    /// <summary>
    /// 文件路径（S3 或本地）
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// 文档名称
    /// </summary>
    public required string DocumentName { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 存储类型
    /// </summary>
    public StorageType StorageType { get; set; }
}
