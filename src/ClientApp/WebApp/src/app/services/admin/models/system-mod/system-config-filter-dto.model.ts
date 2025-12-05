/**
 * 系统配置查询筛选
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
}
