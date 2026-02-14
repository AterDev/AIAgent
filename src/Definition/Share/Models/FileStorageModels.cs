namespace Share.Models;

/// <summary>
/// 文件上传结果
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// 文件路径（本地相对路径或云存储对象键）
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// 访问URL（云存储签名URL）
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 是否为云存储
    /// </summary>
    public bool IsCloud { get; set; }

    /// <summary>
    /// 存储服务商ID
    /// </summary>
    public Guid StorageProviderId { get; set; }
}

/// <summary>
/// 存储提供商基本信息
/// </summary>
public class StorageProviderInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 是否为云存储
    /// </summary>
    public bool IsCloud { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public bool IsActive { get; set; }
}