import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商 AddDto
 */
export interface ModelProviderAddDto {
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
