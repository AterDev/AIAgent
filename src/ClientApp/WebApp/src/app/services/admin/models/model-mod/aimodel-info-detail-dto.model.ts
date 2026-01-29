/**
 * 模型信息DetailDto
 */
export interface AIModelInfoDetailDto {
  /** id */
  id: string;
  /** 上下文长度（tokens） */
  contextLength?: number | null;
  /** createdTime */
  createdTime: Date;
  /** 说明 */
  description?: string | null;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice?: number | null;
  /** 模型名称 */
  name?: string | null;
  /** outputPrice */
  outputPrice?: number | null;
  /** 所属提供商 Id */
  providerId?: string | null;
  /** tenantId */
  tenantId: string;
  /** updatedTime */
  updatedTime: Date;
}
