/**
 * 角色更新时请求结构
 */
export interface SystemRoleUpdateDto {
  /** 角色名称 */
  name?: string | null;
  /** 角色标识 */
  nameValue?: string | null;
  /** 是否系统内置 */
  isSystem?: boolean | null;
}
