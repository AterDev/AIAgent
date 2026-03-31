/**
 * 新增应用 ApiKey 返回结果（仅创建时返回明文）
 */
export interface ApplicationApiKeyCredentialResultDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** applicationName */
  applicationName: string;
  /** name */
  name: string;
  /** apiKey */
  apiKey: string;
  /** keyUpdatedTime */
  keyUpdatedTime: Date;
  /** keyExpiresAt */
  keyExpiresAt: Date;
}
