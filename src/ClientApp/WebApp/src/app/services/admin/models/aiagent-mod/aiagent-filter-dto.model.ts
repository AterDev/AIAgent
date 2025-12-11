/**
 * agentFilterDto
 */
export interface AIAgentFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** is enabled */
  enable?: boolean | null;
  /** isTemplate */
  isTemplate?: boolean | null;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId?: string | null;
  /** Agent 名称 */
  name?: string | null;
  /** userId */
  userId?: string | null;
}
