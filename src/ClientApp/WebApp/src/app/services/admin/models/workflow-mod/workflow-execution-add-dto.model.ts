import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行 AddDto
 */
export interface WorkflowExecutionAddDto {
  /** workflowId */
  workflowId: string;
  /** inputJson */
  inputJson?: string | null;
  /** status */
  status: WorkflowExecutionStatus;
}
