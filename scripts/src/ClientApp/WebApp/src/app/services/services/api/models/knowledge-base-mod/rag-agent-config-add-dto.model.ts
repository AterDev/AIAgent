/**
 * RAG 模型配置AddDto
 */
export interface RagAgentConfigAddDto {
  /** 配置项名称 */
  key: string;
  /** 配置项值 */
  value: string;
  /** 关联的 AI 模型 ID */
  aiModelInfoId?: string | null;
  /** 配置项描述 */
  description?: string | null;
  /** 关联的提示词 ID */
  aiPromptId?: string | null;
}
