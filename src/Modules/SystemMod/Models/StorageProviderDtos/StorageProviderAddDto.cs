namespace SystemMod.Models.StorageProviderDtos;
/// <summary>
/// 存储服务商AddDto
/// </summary>
/// <see cref="StorageProvider"/>
public class StorageProviderAddDto
{
    /// <summary>
    /// 存储服务商名称
    /// </summary>
    [MaxLength(60)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 是否为云存储
    /// </summary>
    public bool IsCloud { get; set; }
    /// <summary>
    /// 本地存储路径
    /// </summary>
    [MaxLength(200)]
    public string? Path { get; set; }
    /// <summary>
    /// 访问端点
    /// </summary>
    [MaxLength(200)]
    public string? Endpoint { get; set; }
    /// <summary>
    /// 访问密钥ID
    /// </summary>
    [MaxLength(100)]
    public string? AccessKeyId { get; set; }
    /// <summary>
    /// 访问密钥密码
    /// </summary>
    [MaxLength(100)]
    public string? AccessKeySecret { get; set; }
    /// <summary>
    /// 存储桶名称
    /// </summary>
    [MaxLength(100)]
    public string? BucketName { get; set; }
    /// <summary>
    /// 存储区域
    /// </summary>
    [MaxLength(100)]
    public string? Region { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; }
    
}
