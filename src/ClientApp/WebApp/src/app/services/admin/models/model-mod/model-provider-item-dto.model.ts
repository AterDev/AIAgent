import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商 ItemDto
 */
export interface ModelProviderItemDto {
  /** id */
  id: string;
  /** name */
  name?: string | null;
  /** baseUrl */
  baseUrl?: string | null;
  /** providerType */
  providerType: ModelProviderType;
  /** isEnabled */
  isEnabled: boolean;
}
