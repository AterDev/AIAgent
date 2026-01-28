import { ModelProvider } from '../entity/model-provider.model';

/**
 * 模型元数据与能力
 */
export interface ModelProfile {
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
  /** providerId */
  providerId: string;
  /** 模型提供商/渠道配置 */
  provider: ModelProvider;
  /** name */
  name: string;
  /** displayName */
  displayName?: string | null;
  /** description */
  description?: string | null;
  /** maxContextTokens */
  maxContextTokens: number;
  /** supportsChat */
  supportsChat: boolean;
  /** supportsEmbedding */
  supportsEmbedding: boolean;
  /** supportsTools */
  supportsTools: boolean;
  /** supportsVision */
  supportsVision: boolean;
  /** supportsResponsesApi */
  supportsResponsesApi: boolean;
  /** isEnabled */
  isEnabled: boolean;
}
