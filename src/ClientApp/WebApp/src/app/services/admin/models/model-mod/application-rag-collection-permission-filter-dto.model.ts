/**
 * 应用知识库关联 FilterDto
 */
export interface ApplicationRagCollectionPermissionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** applicationId */
  applicationId?: string | null;
  /** ragCollectionId */
  ragCollectionId?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}