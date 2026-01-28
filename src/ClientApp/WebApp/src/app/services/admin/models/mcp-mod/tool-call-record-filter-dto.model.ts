import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * 工具调用记录 FilterDto
 */
export interface ToolCallRecordFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** toolId */
  toolId?: string | null;
  /** status */
  status?: ToolCallStatus | null;
}
