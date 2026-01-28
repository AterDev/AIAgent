import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * 工具调用记录 DetailDto
 */
export interface ToolCallRecordDetailDto {
  /** id */
  id: string;
  /** toolId */
  toolId: string;
  /** applicationId */
  applicationId?: string | null;
  /** agentId */
  agentId?: string | null;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
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
