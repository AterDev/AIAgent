/**
 * 应用工具权限 DetailDto
 */
export interface ApplicationToolPermissionDetailDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** toolName */
  toolName?: string | null;
  /** isEnabled */
  isEnabled: boolean;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
