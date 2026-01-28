import { Application } from '../entity/application.model';
import { ModelProfile } from '../entity/model-profile.model';
import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 模型调用记录
 */
export interface ModelInvocation {
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
  /** scene */
  scene: string;
  /** promptTokens */
  promptTokens: number;
  /** completionTokens */
  completionTokens: number;
  /** totalTokens */
  totalTokens: number;
  /** durationMs */
  durationMs: number;
  /** status */
  status: InvocationStatus;
  /** errorMessage */
  errorMessage?: string | null;
}
