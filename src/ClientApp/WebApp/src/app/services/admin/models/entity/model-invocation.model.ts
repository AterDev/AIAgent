import { Application } from '../entity/application.model';
import { AIModelInfo } from '../entity/aimodel-info.model';
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
  /** aiModelInfoId */
  aiModelInfoId: string;
  /** 模型信息（包含能力和定价） */
  aiModelInfo: AIModelInfo;
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
