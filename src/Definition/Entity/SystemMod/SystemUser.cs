namespace Entity.SystemMod;

/// <summary>
/// 系统用户
/// </summary>
[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class SystemUser : EntityBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(60)]
    public required string UserName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    public required string Email { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(100)]
    public string? RealName { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    [MaxLength(200)]
    public required string PasswordHash { get; set; }

    /// <summary>
    /// 密码盐
    /// </summary>
    [MaxLength(60)]
    public required string PasswordSalt { get; set; }

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
