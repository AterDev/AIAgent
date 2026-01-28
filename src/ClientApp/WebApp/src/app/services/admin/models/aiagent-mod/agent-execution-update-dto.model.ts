import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行 UpdateDto
 */
export interface AgentExecutionUpdateDto {
  /** outputJson */
  outputJson?: string | null;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs?: number | null;
  /** status */
  status?: AgentExecutionStatus | null;
  /** errorMessage */
  errorMessage?: string | null;
}
