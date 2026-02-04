/**
 * 系统配置DetailDto
 */
export interface SystemConfigDetailDto {
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
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
