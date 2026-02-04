import { Application } from '../entity/application.model';
import { AIModelInfo } from '../entity/aimodel-info.model';

/**
 * 应用模型权限
 */
export interface ApplicationModelPermission {
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
  /** applicationId */
  applicationId: string;
  /** 应用定义 */
  application: Application;
  /** aiModelInfoId */
  aiModelInfoId: string;
  /** 模型信息（包含能力和定价） */
  aiModelInfo: AIModelInfo;
  /** isEnabled */
  isEnabled: boolean;
}
