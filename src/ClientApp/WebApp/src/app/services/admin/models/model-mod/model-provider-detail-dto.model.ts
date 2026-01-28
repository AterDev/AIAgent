import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商 DetailDto
 */
export interface ModelProviderDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
  /** baseUrl */
  baseUrl?: string | null;
  /** apiKey */
  apiKey?: string | null;
  /** providerType */
  providerType: ModelProviderType;
  /** timeoutSeconds */
  timeoutSeconds: number;
  /** retryCount */
  retryCount: number;
  /** isEnabled */
  isEnabled: boolean;
  /** description */
  description?: string | null;
}
