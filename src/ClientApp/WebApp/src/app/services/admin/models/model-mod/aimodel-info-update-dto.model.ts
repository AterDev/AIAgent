/**
 * 模型信息UpdateDto
 */
export interface AIModelInfoUpdateDto {
  /** 上下文长度（tokens） */
  contextLength?: number | null;
  /** 说明 */
  description?: string | null;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice?: number | null;
  /** 模型名称 */
  name?: string | null;
  /** outputPrice */
  outputPrice?: number | null;
  /** providerId */
  providerId?: string | null;
}
