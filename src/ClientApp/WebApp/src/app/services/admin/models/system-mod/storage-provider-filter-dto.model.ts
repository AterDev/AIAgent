/**
 * 存储服务商FilterDto
 */
export interface StorageProviderFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 存储服务商名称 */
  name?: string | null;
  /** 是否为云存储 */
  isCloud?: boolean | null;
  /** 是否启用 */
  isActive?: boolean | null;
}
