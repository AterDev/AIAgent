/**
 * 模型信息UpdateDto
 */
export interface AIModelInfoUpdateDto {
  /** 模型名称 */
  name?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 说明 */
  description?: string | null;
  /** 所属提供商 Id */
  providerId?: string | null;
  /** 上下文长度（tokens） */
  contextLength?: number | null;
  /** 最大上下文长度（tokens） */
  maxContextTokens?: number | null;
  /** 支持聊天 */
  supportsChat?: boolean | null;
  /** 支持向量化 */
  supportsEmbedding?: boolean | null;
  /** 支持工具调用 */
  supportsTools?: boolean | null;
  /** 支持视觉 */
  supportsVision?: boolean | null;
  /** 支持 Responses API */
  supportsResponsesApi?: boolean | null;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice?: number | null;
  /** 价格（单位: 每 1k tokens 的价格） */
  outputPrice?: number | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
}
