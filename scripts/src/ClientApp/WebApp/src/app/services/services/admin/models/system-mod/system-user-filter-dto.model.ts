/**
 * 系统用户FilterDto
 */
export interface SystemUserFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 用户名 */
  userName?: string | null;
  /** 邮箱 */
  email?: string | null;
  /** 是否启用 */
  enabled?: boolean | null;
}
