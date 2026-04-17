/**
 * 应用知识库关联 DetailDto
 */
export interface ApplicationRagCollectionPermissionDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** applicationId */
  applicationId: string;
  /** ragCollectionId */
  ragCollectionId: string;
  /** isEnabled */
  isEnabled: boolean;
}
