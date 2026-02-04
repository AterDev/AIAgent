import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行 UpdateDto
 */
export interface WorkflowExecutionUpdateDto {
  /** outputJson */
  outputJson?: string | null;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs?: number | null;
  /** status */
  status?: WorkflowExecutionStatus | null;
  /** errorMessage */
  errorMessage?: string | null;
}
