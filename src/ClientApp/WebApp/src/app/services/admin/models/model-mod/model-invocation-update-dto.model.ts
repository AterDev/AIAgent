import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 调用记录 UpdateDto
 */
export interface ModelInvocationUpdateDto {
  /** scene */
  scene?: string | null;
  /** promptTokens */
  promptTokens?: number | null;
  /** completionTokens */
  completionTokens?: number | null;
  /** totalTokens */
  totalTokens?: number | null;
  /** durationMs */
  durationMs?: number | null;
  /** status */
  status?: InvocationStatus | null;
  /** errorMessage */
  errorMessage?: string | null;
}
