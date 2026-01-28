import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 调用记录 DetailDto
 */
export interface ModelInvocationDetailDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** modelProfileId */
  modelProfileId: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** scene */
  scene?: string | null;
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
