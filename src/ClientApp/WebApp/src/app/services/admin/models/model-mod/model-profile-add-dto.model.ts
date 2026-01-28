/**
 * 模型配置 AddDto
 */
export interface ModelProfileAddDto {
  /** providerId */
  providerId: string;
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
