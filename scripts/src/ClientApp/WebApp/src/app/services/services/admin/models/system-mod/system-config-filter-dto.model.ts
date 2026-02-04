/**
 * 系统配置FilterDto
 */
export interface SystemConfigFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** key */
  key?: string | null;
  /** 组 */
  groupName?: string | null;
  /** valid */
  valid?: boolean | null;
  /** 是否属于系统配置 */
  isSystem?: boolean | null;
}
