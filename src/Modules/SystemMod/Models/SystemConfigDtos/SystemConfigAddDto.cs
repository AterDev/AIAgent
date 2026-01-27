using Entity.SystemMod;
namespace SystemMod.Models.SystemConfigDtos;
/// <summary>
/// 系统配置AddDto
/// </summary>
/// <see cref="Entity.SystemMod.SystemConfig"/>
public class SystemConfigAddDto
{
    [MaxLength(100)]
    public string Key { get; set; } = default!;
    /// <summary>
    /// 以json字符串形式存储
    /// </summary>
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
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
    
}
