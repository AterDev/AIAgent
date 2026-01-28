import { Workflow } from '../entity/workflow.model';
import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';

/**
 * 工作流执行记录
 */
export interface WorkflowExecution {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** workflowId */
  workflowId: string;
  /** 工作流定义 */
  workflow: Workflow;
  /** status */
  status: WorkflowExecutionStatus;
  /** inputJson */
  inputJson: string;
  /** outputJson */
  outputJson: string;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs: number;
  /** errorMessage */
  errorMessage?: string | null;
}
