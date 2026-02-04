import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行 DetailDto
 */
export interface AgentExecutionDetailDto {
  /** id */
  id: string;
  /** agentId */
  agentId: string;
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
  /** status */
  status: AgentExecutionStatus;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs: number;
  /** errorMessage */
  errorMessage?: string | null;
}
