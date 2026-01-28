/**
 * 模型信息FilterDto
 */
export interface AIModelInfoFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** providerId */
  providerId?: string | null;
}
