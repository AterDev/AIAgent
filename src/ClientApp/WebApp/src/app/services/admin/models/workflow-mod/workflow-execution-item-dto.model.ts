import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行 ItemDto
 */
export interface WorkflowExecutionItemDto {
  /** id */
  id: string;
  /** workflowId */
  workflowId: string;
  /** status */
  status: WorkflowExecutionStatus;
  /** durationMs */
  durationMs: number;
}
