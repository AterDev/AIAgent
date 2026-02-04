using Entity.SystemMod;
namespace SystemMod.Models.StorageProviderDtos;
/// <summary>
/// 存储服务商FilterDto
/// </summary>
/// <see cref="Entity.SystemMod.StorageProvider"/>
public class StorageProviderFilterDto : FilterBase
{
    /// <summary>
    /// 存储服务商名称
    /// </summary>
    [MaxLength(60)]
    public string? Name { get; set; }
    /// <summary>
    /// 是否为云存储
    /// </summary>
    public bool? IsCloud { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsActive { get; set; }
    
}
