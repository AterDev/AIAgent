namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 登录DTO
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required, MaxLength(60)]
    public required string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required, MinLength(6), MaxLength(30)]
    public required string Password { get; set; }
}
