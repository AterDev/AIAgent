/**
 * 应用定义FilterDto
 */
export interface ApplicationFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
