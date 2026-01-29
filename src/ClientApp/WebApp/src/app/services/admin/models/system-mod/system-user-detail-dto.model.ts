/**
 * 系统用户DetailDto
 */
export interface SystemUserDetailDto {
  /** 用户名 */
  userName: string;
  /** 邮箱 */
  email: string;
  /** 真实姓名 */
  realName?: string | null;
  /** 角色(多个角色用,分隔) */
  roles?: string | null;
  /** 是否启用 */
  enabled: boolean;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** 头像 */
  avatar?: string | null;
  /** 电话 */
  phone?: string | null;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
