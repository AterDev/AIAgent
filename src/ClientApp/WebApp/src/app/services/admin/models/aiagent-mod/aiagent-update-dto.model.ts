/**
 * agentUpdateDto
 */
export interface AIAgentUpdateDto {
  /** Agent 描述信息 */
  description?: string | null;
  /** is enabled */
  enable?: boolean | null;
  /** isTemplate */
  isTemplate?: boolean | null;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId?: string | null;
  /** Agent 名称 */
  name?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt?: string | null;
  /** Agent 可用的工具列表 */
  tools?: string[] | null;
  /** userId */
  userId?: string | null;
}
