/**
 * 系统配置概要
 */
export interface SystemConfigDetailDto {
  /** key */
  key: string;
  /** description */
  description?: string | null;
  /** valid */
  valid: boolean;
  /** 是否属于系统配置 */
  isSystem: boolean;
  /** 组 */
  groupName?: string | null;
}
