/**
 * 应用知识库关联 AddDto
 */
export interface ApplicationRagCollectionPermissionAddDto {
  /** applicationId */
  applicationId: string;
  /** ragCollectionId */
  ragCollectionId: string;
  /** isEnabled */
  isEnabled: boolean;
}