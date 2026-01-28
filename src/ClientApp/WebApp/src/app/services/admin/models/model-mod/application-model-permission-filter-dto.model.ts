/**
 * 应用模型权限 FilterDto
 */
export interface ApplicationModelPermissionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** applicationId */
  applicationId?: string | null;
  /** modelProfileId */
  modelProfileId?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
