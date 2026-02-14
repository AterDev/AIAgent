namespace SystemMod.Models.SystemConfigDtos;
/// <summary>
/// 系统配置ItemDto
/// </summary>
/// <see cref="SystemConfig"/>
public class SystemConfigItemDto
{
    [MaxLength(100)]
    public string Key { get; set; } = default!;
    public bool Valid { get; set; } = true;
    /// <summary>
    /// 是否属于系统配置
    /// </summary>
    public bool IsSystem { get; set; }
    /// <summary>
    /// 组
    /// </summary>
    [MaxLength(60)]
    public string GroupName { get; set; } = string.Empty;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

}
