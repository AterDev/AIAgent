namespace SystemMod.Models.StorageProviderDtos;
/// <summary>
/// 存储服务商ItemDto
/// </summary>
/// <see cref="StorageProvider"/>
public class StorageProviderItemDto
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
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    
}
