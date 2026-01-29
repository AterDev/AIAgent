import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 调用记录 AddDto
 */
export interface ModelInvocationAddDto {
  /** applicationId */
  applicationId: string;
  /** aiModelInfoId */
  aiModelInfoId: string;
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
