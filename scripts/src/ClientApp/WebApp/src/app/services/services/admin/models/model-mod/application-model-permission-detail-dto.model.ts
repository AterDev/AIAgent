/**
 * 应用模型权限 DetailDto
 */
export interface ApplicationModelPermissionDetailDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** aiModelInfoId */
  aiModelInfoId: string;
  /** isEnabled */
  isEnabled: boolean;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
