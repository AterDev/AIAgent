export interface ModelDebugRequest {
  /** applicationId */
  applicationId?: string | null;
  /** modelId */
  modelId: string;
  /** provider */
  provider?: string | null;
  /** systemPrompt */
  systemPrompt?: string | null;
  /** prompt */
  prompt: string;
  /** temperature */
  temperature?: number | null;
  /** maxTokens */
  maxTokens?: number | null;
  /** requestId */
  requestId?: string | null;
}
