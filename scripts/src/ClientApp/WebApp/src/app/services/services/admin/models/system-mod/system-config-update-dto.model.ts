/**
 * 系统配置UpdateDto
 */
export interface SystemConfigUpdateDto {
  /** key */
  key?: string | null;
  /** 以json字符串形式存储 */
  value?: string | null;
  /** description */
  description?: string | null;
  /** valid */
  valid?: boolean | null;
  /** 是否属于系统配置 */
  isSystem?: boolean | null;
  /** 组 */
  groupName?: string | null;
}
