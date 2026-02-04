import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 调用记录 ItemDto
 */
export interface ModelInvocationItemDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** aiModelInfoId */
  aiModelInfoId: string;
  /** scene */
  scene?: string | null;
  /** totalTokens */
  totalTokens: number;
  /** durationMs */
  durationMs: number;
  /** status */
  status: InvocationStatus;
}
