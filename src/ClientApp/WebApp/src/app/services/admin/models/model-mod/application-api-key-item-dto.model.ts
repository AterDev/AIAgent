/**
 * 应用 ApiKey 列表项
 */
export interface ApplicationApiKeyItemDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** name */
  name: string;
  /** keyUpdatedTime */
  keyUpdatedTime: Date;
  /** keyExpiresAt */
  keyExpiresAt: Date;
  /** isExpired */
  isExpired: boolean;
  /** createdTime */
  createdTime: Date;
}
