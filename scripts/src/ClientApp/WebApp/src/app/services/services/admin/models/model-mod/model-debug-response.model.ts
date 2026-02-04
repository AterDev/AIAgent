export interface ModelDebugResponse {
  /** content */
  content: string;
  /** model */
  model: string;
  /** promptTokens */
  promptTokens: number;
  /** completionTokens */
  completionTokens: number;
  /** totalTokens */
  totalTokens: number;
  /** finishReason */
  finishReason: string;
  /** durationMs */
  durationMs: number;
}
