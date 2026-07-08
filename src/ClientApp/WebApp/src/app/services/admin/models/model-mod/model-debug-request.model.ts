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
  /** dimensions */
  dimensions?: number | null;
  /** 多模态图片输入（data URI 或 http(s) URL） */
  images: string[];
  /** requestId */
  requestId?: string | null;
}
