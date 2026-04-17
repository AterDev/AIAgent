import { AgentCapabilities } from '../entity/agent-capabilities.model';
import { AgentMemoryMode } from '../entity/agent-memory-mode.model';

/**
 * agentAddDto
 */
export interface AIAgentAddDto {
  /** Agent 名称 */
  name: string;
  /** Agent 描述信息 */
  description: string;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId: string;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt: string;
  /** Agent 可用的工具列表 */
  tools: string[];
  /** 可 handoff 的目标 Agent 名称列表 */
  handoffTargets: string[];
  /** Skill 名称列表 */
  skills: string[];
  /** 标签 */
  tags: string[];
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
  /** 结构化输出 JSON Schema（可选） */
  responseSchemaJson?: string | null;
  /** providerId */
  providerId?: string | null;
  /** is enabled */
  enable: boolean;
  /** isPublic */
  isPublic: boolean;
  /** applicationId */
  applicationId?: string | null;
}
