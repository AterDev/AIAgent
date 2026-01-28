import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * 工具调用记录 UpdateDto
 */
export interface ToolCallRecordUpdateDto {
  /** outputJson */
  outputJson?: string | null;
  /** durationMs */
  durationMs?: number | null;
  /** status */
  status?: ToolCallStatus | null;
  /** errorMessage */
  errorMessage?: string | null;
}
