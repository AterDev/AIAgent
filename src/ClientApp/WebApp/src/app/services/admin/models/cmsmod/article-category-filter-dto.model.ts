/**
 * 目录查询筛选
 */
export interface ArticleCategoryFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 目录名称 */
  name?: string | null;
}
