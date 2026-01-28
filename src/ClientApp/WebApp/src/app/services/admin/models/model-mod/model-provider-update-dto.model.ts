import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商 UpdateDto
 */
export interface ModelProviderUpdateDto {
  /** name */
  name?: string | null;
  /** baseUrl */
  baseUrl?: string | null;
  /** apiKey */
  apiKey?: string | null;
  /** providerType */
  providerType?: ModelProviderType | null;
  /** timeoutSeconds */
  timeoutSeconds?: number | null;
  /** retryCount */
  retryCount?: number | null;
  /** isEnabled */
  isEnabled?: boolean | null;
  /** description */
  description?: string | null;
}
