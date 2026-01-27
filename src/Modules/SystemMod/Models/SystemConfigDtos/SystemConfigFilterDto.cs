using Entity.SystemMod;
namespace SystemMod.Models.SystemConfigDtos;
/// <summary>
/// 系统配置FilterDto
/// </summary>
/// <see cref="Entity.SystemMod.SystemConfig"/>
public class SystemConfigFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Key { get; set; }
    /// <summary>
    /// 组
    /// </summary>
    [MaxLength(60)]
    public string? GroupName { get; set; }
    public bool? Valid { get; set; }
    /// <summary>
    /// 是否属于系统配置
    /// </summary>
    public bool? IsSystem { get; set; }
    
}
