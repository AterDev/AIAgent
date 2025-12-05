/**
 * 角色添加时请求结构
 */
export interface SystemRoleAddDto {
  /** 角色名称 */
  name: string;
  /** 角色标识 */
  nameValue: string;
  /** 是否系统内置 */
  isSystem: boolean;
}
