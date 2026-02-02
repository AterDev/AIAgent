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
  /** requestId */
  requestId?: string | null;
}
