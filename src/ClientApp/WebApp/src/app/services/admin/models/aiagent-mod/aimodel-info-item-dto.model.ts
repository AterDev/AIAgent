/**
 * 模型信息ItemDto
 */
export interface AIModelInfoItemDto {
  /** id */
  id: string;
  /** 上下文长度（tokens） */
  contextLength?: number | null;
  /** createdTime */
  createdTime: Date;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice?: number | null;
  /** outputPrice */
  outputPrice?: number | null;
}
