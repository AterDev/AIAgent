/**
 * 模型信息AddDto
 */
export interface AIModelInfoAddDto {
  /** 模型名称 */
  name: string;
  /** 显示名称 */
  displayName?: string | null;
  /** 说明 */
  description?: string | null;
  /** 所属提供商 Id */
  providerId: string;
  /** 上下文长度（tokens） */
  contextLength: number;
  /** 最大上下文长度（tokens） */
  maxContextTokens: number;
  /** 支持聊天 */
  supportsChat: boolean;
  /** 支持向量化 */
  supportsEmbedding: boolean;
  /** 支持工具调用 */
  supportsTools: boolean;
  /** 支持视觉 */
  supportsVision: boolean;
  /** 支持 Responses API */
  supportsResponsesApi: boolean;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice: number;
  /** 价格（单位: 每 1k tokens 的价格） */
  outputPrice: number;
  /** 是否启用 */
  isEnabled: boolean;
}
