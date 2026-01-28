/**
 * 分块 FilterDto
 */
export interface RagChunkFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** documentId */
  documentId?: string | null;
}
