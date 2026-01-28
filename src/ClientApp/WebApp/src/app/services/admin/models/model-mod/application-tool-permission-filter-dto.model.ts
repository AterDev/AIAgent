/**
 * 应用工具权限 FilterDto
 */
export interface ApplicationToolPermissionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** applicationId */
  applicationId?: string | null;
  /** toolName */
  toolName?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
