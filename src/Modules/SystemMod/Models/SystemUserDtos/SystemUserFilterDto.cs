namespace SystemMod.Models.SystemUserDtos;
/// <summary>
/// 系统用户FilterDto
/// </summary>
/// <see cref="SystemUser"/>
public class SystemUserFilterDto : FilterBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(60)]
    public string? UserName { get; set; }
    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enabled { get; set; }
    
}
