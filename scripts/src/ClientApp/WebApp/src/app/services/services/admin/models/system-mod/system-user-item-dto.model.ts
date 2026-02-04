/**
 * 系统用户ItemDto
 */
export interface SystemUserItemDto {
  /** 用户名 */
  userName: string;
  /** 邮箱 */
  email: string;
  /** 真实姓名 */
  realName?: string | null;
  /** 是否启用 */
  enabled: boolean;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** 电话 */
  phone?: string | null;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
}
