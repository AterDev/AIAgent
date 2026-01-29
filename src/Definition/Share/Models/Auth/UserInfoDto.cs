namespace Share.Models.Auth;

/// <summary>
/// 用户信息DTO
/// </summary>
public class UserInfoDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }
    
    /// <summary>
    /// 邮箱
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName { get; set; }
    
    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// 角色(多个角色用,分隔)
    /// </summary>
    public string? Roles { get; set; }
}
