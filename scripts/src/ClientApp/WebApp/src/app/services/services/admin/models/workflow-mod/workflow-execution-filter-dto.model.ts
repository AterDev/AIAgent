import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行 FilterDto
 */
export interface WorkflowExecutionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** workflowId */
  workflowId?: string | null;
  /** status */
  status?: WorkflowExecutionStatus | null;
}
