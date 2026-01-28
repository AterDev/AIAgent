/**
 * 模型配置 FilterDto
 */
export interface ModelProfileFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** providerId */
  providerId?: string | null;
  /** name */
  name?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
