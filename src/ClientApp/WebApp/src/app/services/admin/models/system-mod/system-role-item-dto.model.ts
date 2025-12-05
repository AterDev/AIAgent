/**
 * 角色列表元素
 */
export interface SystemRoleItemDto {
  /** id */
  id: string;
  /** 角色名称 */
  name: string;
  /** 角色标识 */
  nameValue: string;
  /** 是否系统内置 */
  isSystem: boolean;
  /** createdTime */
  createdTime: Date;
}
