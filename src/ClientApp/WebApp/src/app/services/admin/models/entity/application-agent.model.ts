import { AgentCapabilities } from '../entity/agent-capabilities.model';
import { AgentMemoryMode } from '../entity/agent-memory-mode.model';
import { Application } from '../entity/application.model';

/**
 * 应用侧 Agent
 */
export interface ApplicationAgent {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
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
  /** Agent 关联的 Skill 名称列表 */
  skills: string[];
  /** 可 Handoff 的目标 Agent 名称列表 */
  handoffTargets: string[];
  /** Agent 能力标志（按位组合） */
  capabilities: AgentCapabilities;
  /** Agent 记忆模式 */
  memoryMode: AgentMemoryMode;
  /** 上下文窗口 */
  contextWindow: number;
  /** 采样温度 */
  temperature?: number | null;
  /** TopP */
  topP?: number | null;
  /** 最大输出 token 数 */
  maxOutputTokens?: number | null;
  /** 结构化输出的 JSON Schema（可选） */
  responseSchemaJson?: string | null;
  /** is enabled */
  enable: boolean;
  /** applicationId */
  applicationId: string;
  /** 应用定义 */
  application: Application;
  /** userId */
  userId?: string | null;
}
