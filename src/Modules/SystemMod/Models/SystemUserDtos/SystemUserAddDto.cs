namespace SystemMod.Models.SystemUserDtos;
/// <summary>
/// 系统用户AddDto
/// </summary>
/// <see cref="SystemUser"/>
public class SystemUserAddDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(60)]
    public string UserName { get; set; } = default!;
    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    public string Email { get; set; } = default!;
    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(100)]
    public string? RealName { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    [Required, MinLength(6), MaxLength(30)]
    public string Password { get; set; } = default!;
    /// <summary>
    /// 角色(多个角色用,分隔)
    /// </summary>
    [MaxLength(200)]
    public string? Roles { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTimeOffset? LastLoginTime { get; set; }
    /// <summary>
    /// 头像
    /// </summary>
    [MaxLength(500)]
    public string? Avatar { get; set; }
    /// <summary>
    /// 电话
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }
    
}
