/**
 * 应用模型权限 AddDto
 */
export interface ApplicationModelPermissionAddDto {
  /** applicationId */
  applicationId: string;
  /** modelProfileId */
  modelProfileId: string;
  /** isEnabled */
  isEnabled: boolean;
}
