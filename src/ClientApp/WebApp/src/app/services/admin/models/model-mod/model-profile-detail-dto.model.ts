/**
 * 模型配置 DetailDto
 */
export interface ModelProfileDetailDto {
  /** id */
  id: string;
  /** providerId */
  providerId: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
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
