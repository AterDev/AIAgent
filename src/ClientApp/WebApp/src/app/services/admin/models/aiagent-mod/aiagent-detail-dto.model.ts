import { AgentCapabilities } from '../entity/agent-capabilities.model';
import { AgentMemoryMode } from '../entity/agent-memory-mode.model';

/**
 * agentDetailDto
 */
export interface AIAgentDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** applicationId */
  applicationId?: string | null;
  /** Agent 描述信息 */
  description?: string | null;
  /** is enabled */
  enable?: boolean | null;
  /** isPublic */
  isPublic?: boolean | null;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId?: string | null;
  /** Agent 名称 */
  name?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt?: string | null;
  /** tenantId */
  tenantId: string;
  /** Agent 可用的工具列表 */
  tools?: string[] | null;
  /** handoffTargets */
  handoffTargets?: string[] | null;
  /** skills */
  skills?: string[] | null;
  /** tags */
  tags?: string[] | null;
  /** Agent 能力标志（按位组合） */
  capabilities: AgentCapabilities;
  /** Agent 记忆模式 */
  memoryMode: AgentMemoryMode;
  /** contextWindow */
  contextWindow?: number | null;
  /** temperature */
  temperature?: number | null;
  /** topP */
  topP?: number | null;
  /** maxOutputTokens */
  maxOutputTokens?: number | null;
  /** responseSchemaJson */
  responseSchemaJson?: string | null;
  /** providerId */
  providerId?: string | null;
  /** updatedTime */
  updatedTime: Date;
}
