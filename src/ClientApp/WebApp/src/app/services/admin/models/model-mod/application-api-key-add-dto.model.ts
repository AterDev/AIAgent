/**
 * 新增应用 ApiKey
 */
export interface ApplicationApiKeyAddDto {
  /** name */
  name: string;
  /** apiKeyExpiresInMonths */
  apiKeyExpiresInMonths: number;
}
