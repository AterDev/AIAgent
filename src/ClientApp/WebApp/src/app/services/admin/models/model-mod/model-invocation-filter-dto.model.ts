import { InvocationStatus } from '../entity/invocation-status.model';

/**
 * 调用记录 FilterDto
 */
export interface ModelInvocationFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** applicationId */
  applicationId?: string | null;
  /** aiModelInfoId */
  aiModelInfoId?: string | null;
  /** scene */
  scene?: string | null;
  /** status */
  status?: InvocationStatus | null;
}
