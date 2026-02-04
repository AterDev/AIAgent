import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * 工具调用记录 ItemDto
 */
export interface ToolCallRecordItemDto {
  /** id */
  id: string;
  /** toolId */
  toolId: string;
  /** status */
  status: ToolCallStatus;
  /** durationMs */
  durationMs: number;
}
