/**
 * 修改密码DTO
 */
export interface ChangePasswordDto {
  /** 旧密码 */
  oldPassword: string;
  /** 新密码 */
  newPassword: string;
}
