import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商/渠道配置
 */
export interface ModelProvider {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** name */
  name: string;
  /** baseUrl */
  baseUrl: string;
  /** apiKey */
  apiKey: string;
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
