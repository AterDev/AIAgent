/**
 * 模型信息AddDto
 */
export interface AIModelInfoAddDto {
  /** 上下文长度（tokens） */
  contextLength: number;
  /** 说明 */
  description?: string | null;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice: number;
  /** 模型名称 */
  name: string;
  /** outputPrice */
  outputPrice: number;
  /** providerId */
  providerId: string;
}
