/**
 * 应用凭证返回（仅创建/重置时返回明文密钥）
 */
export interface ApplicationCredentialResultDto {
  /** id */
  id: string;
  /** name */
  name: string;
  /** clientId */
  clientId: string;
  /** clientSecret */
  clientSecret: string;
  /** isEnabled */
  isEnabled: boolean;
  /** secretUpdatedTime */
  secretUpdatedTime: Date;
}