import { AgentCapabilities } from '../entity/agent-capabilities.model';
import { AgentMemoryMode } from '../entity/agent-memory-mode.model';

/**
 * AI Agent 定义（基于 Microsoft Agent Framework 1.1 的 ChatClientAgent）
 */
export interface AIAgent {
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
  /** Agent 所使用的大模型名称（例如 "gpt-4.1", "deepseek-chat"）。
  留空时使用 Entity.AIAgentMod.AIAgent.ProviderId + 默认模型。 */
  modelId: string;
  /** 可选：绑定的模型提供商 Id（为空时根据 ModelId 自动解析） */
  providerId?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt: string;
  /** Agent 可用的工具名称列表（MCP / 内置） */
  tools: string[];
  /** Agent 关联的 Skill 名称列表（由 AIFunctionFactory 暴露的业务函数） */
  skills: string[];
  /** 可 Handoff 的目标 Agent 名称列表（供工作流/对话编排使用） */
  handoffTargets: string[];
  /** 标签列表 */
  tags: string[];
  /** Agent 能力标志（按位组合） */
  capabilities: AgentCapabilities;
  /** Agent 记忆模式 */
  memoryMode: AgentMemoryMode;
  /** 上下文窗口（历史消息保留条数，对 Window/Summary 模式有效） */
  contextWindow: number;
  /** 结构化输出的 JSON Schema（可选） */
  responseSchemaJson?: string | null;
  /** 采样温度 */
  temperature?: number | null;
  /** TopP */
  topP?: number | null;
  /** 最大输出 token 数 */
  maxOutputTokens?: number | null;
  /** 频率惩罚 */
  frequencyPenalty?: number | null;
  /** 存在惩罚 */
  presencePenalty?: number | null;
  /** Agent 图标 URL */
  iconUrl?: string | null;
  /** 偏好输出语言（zh-CN、en-US 等） */
  outputLanguage?: string | null;
  /** 是否启用 */
  enable: boolean;
  /** 是否公共（跨租户可用） */
  isPublic: boolean;
}
