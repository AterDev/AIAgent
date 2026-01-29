namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 修改密码DTO
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required, MinLength(6), MaxLength(30)]
    public required string OldPassword { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Required, MinLength(6), MaxLength(30)]
    public required string NewPassword { get; set; }
}
