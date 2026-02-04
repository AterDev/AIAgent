import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * 工具调用记录 AddDto
 */
export interface ToolCallRecordAddDto {
  /** toolId */
  toolId: string;
  /** applicationId */
  applicationId?: string | null;
  /** agentId */
  agentId?: string | null;
  /** inputJson */
  inputJson?: string | null;
  /** outputJson */
  outputJson?: string | null;
  /** durationMs */
  durationMs: number;
  /** status */
  status: ToolCallStatus;
  /** errorMessage */
  errorMessage?: string | null;
}
