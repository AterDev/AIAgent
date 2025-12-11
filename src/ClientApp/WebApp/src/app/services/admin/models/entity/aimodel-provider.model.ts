import { AIModelInfo } from '../entity/aimodel-info.model';

/**
 * AI模型提供商
 */
export interface AIModelProvider {
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
  /** 提供商名称 */
  name: string;
  /** logoUrl */
  logoUrl?: string | null;
  /** 说明 */
  description?: string | null;
  /** 官网地址 */
  website?: string | null;
  /** 关联的模型列表 */
  models: AIModelInfo[];
}
