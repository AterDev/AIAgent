import { AIModelProvider } from '../entity/aimodel-provider.model';

/**
 * 模型信息
 */
export interface AIModelInfo {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** 所属提供商 Id */
  providerId: string;
  /** AI模型提供商 */
  provider: AIModelProvider;
  /** 模型名称 */
  name: string;
  /** 说明 */
  description?: string | null;
  /** 上下文长度（tokens） */
  contextLength: number;
  /** 价格（单位: 每 1k tokens 的价格） */
  inputPrice: number;
  /** outputPrice */
  outputPrice: number;
}
