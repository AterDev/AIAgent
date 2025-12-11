/**
 * agentAddDto
 */
export interface AIAgentAddDto {
  /** Agent 描述信息 */
  description: string;
  /** is enabled */
  enable: boolean;
  /** isTemplate */
  isTemplate: boolean;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId: string;
  /** Agent 名称 */
  name: string;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt: string;
  /** Agent 可用的工具列表 */
  tools: string[];
  /** userId */
  userId?: string | null;
}
