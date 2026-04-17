export interface AgentDebugRequest {
  /** applicationId */
  applicationId?: string | null;
  /** agentId */
  agentId: string;
  /** systemPrompt */
  systemPrompt?: string | null;
  /** userMessage */
  userMessage: string;
  /** temperature */
  temperature?: number | null;
  /** maxTokens */
  maxTokens?: number | null;
  /** enabledTools */
  enabledTools: string[];
  /** enableToolCallLogging */
  enableToolCallLogging: boolean;
  /** 多模态图片输入（data URI 或 http(s) URL） */
  images: string[];
  /** requestId */
  requestId?: string | null;
}
