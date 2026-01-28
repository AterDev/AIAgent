import { ModelProviderType } from '../entity/model-provider-type.model';

/**
 * 模型提供商 FilterDto
 */
export interface ModelProviderFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** providerType */
  providerType?: ModelProviderType | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
