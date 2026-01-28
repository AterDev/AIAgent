import { Application } from '../entity/application.model';
import { ModelProfile } from '../entity/model-profile.model';

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
  /** modelProfileId */
  modelProfileId: string;
  /** 模型元数据与能力 */
  modelProfile: ModelProfile;
  /** isEnabled */
  isEnabled: boolean;
}
