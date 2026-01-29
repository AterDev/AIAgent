/**
 * 用户信息DTO
 */
export interface UserInfoDto {
  /** id */
  id: string;
  /** 用户名 */
  userName: string;
  /** 邮箱 */
  email: string;
  /** 真实姓名 */
  realName?: string | null;
  /** 头像 */
  avatar?: string | null;
  /** 角色(多个角色用,分隔) */
  roles?: string | null;
}
