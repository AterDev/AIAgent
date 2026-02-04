/**
 * 系统用户UpdateDto
 */
export interface SystemUserUpdateDto {
  /** 用户名 */
  userName?: string | null;
  /** 邮箱 */
  email?: string | null;
  /** 真实姓名 */
  realName?: string | null;
  /** 角色(多个角色用,分隔) */
  roles?: string | null;
  /** 是否启用 */
  enabled?: boolean | null;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** 头像 */
  avatar?: string | null;
  /** 电话 */
  phone?: string | null;
}
