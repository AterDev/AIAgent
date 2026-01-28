import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行 DetailDto
 */
export interface WorkflowExecutionDetailDto {
  /** id */
  id: string;
  /** workflowId */
  workflowId: string;
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
  status: WorkflowExecutionStatus;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs: number;
  /** errorMessage */
  errorMessage?: string | null;
}
