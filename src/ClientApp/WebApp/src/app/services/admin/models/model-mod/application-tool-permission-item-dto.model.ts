/**
 * 应用工具权限 ItemDto
 */
export interface ApplicationToolPermissionItemDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** toolName */
  toolName?: string | null;
  /** isEnabled */
  isEnabled: boolean;
}
