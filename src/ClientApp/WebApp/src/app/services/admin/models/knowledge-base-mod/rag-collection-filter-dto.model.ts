/**
 * 知识库 FilterDto
 */
export interface RagCollectionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** isPublic */
  isPublic?: boolean | null;
  /** isEnabled */
  isEnabled?: boolean | null;
  /** applicationId */
  applicationId?: string | null;
}
