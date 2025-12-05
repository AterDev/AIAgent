/**
 * 角色概要
 */
export interface SystemRoleDetailDto {
  /** 角色名称 */
  name: string;
  /** 角色标识 */
  nameValue: string;
  /** 是否系统内置 */
  isSystem: boolean;
  /** 图标 */
  icon?: string | null;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
}
