/**
 * 系统配置
 */
export interface SystemConfig {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** key */
  key: string;
  /** 以json字符串形式存储 */
  value: string;
  /** description */
  description?: string | null;
  /** valid */
  valid: boolean;
  /** 是否属于系统配置 */
  isSystem: boolean;
  /** 组 */
  groupName: string;
}
