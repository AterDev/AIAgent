/**
 * 模型配置 UpdateDto
 */
export interface ModelProfileUpdateDto {
  /** providerId */
  providerId?: string | null;
  /** name */
  name?: string | null;
  /** displayName */
  displayName?: string | null;
  /** description */
  description?: string | null;
  /** maxContextTokens */
  maxContextTokens?: number | null;
  /** supportsChat */
  supportsChat?: boolean | null;
  /** supportsEmbedding */
  supportsEmbedding?: boolean | null;
  /** supportsTools */
  supportsTools?: boolean | null;
  /** supportsVision */
  supportsVision?: boolean | null;
  /** supportsResponsesApi */
  supportsResponsesApi?: boolean | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
