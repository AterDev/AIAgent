/**
 * 批量同步应用模型权限
 */
export interface ApplicationModelPermissionSyncDto {
  /** applicationId */
  applicationId: string;
  /** aiModelInfoIds */
  aiModelInfoIds: string[];
}