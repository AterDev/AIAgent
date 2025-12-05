/**
 * 系统配置列表元素
 */
export interface SystemConfigItemDto {
  /** id */
  id: string;
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
  /** 以json字符串形式存储 */
  value?: string | null;
}
