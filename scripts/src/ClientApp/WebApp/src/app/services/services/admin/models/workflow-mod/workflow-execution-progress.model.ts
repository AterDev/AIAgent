import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';
import { StepExecutionInfo } from '../workflow-mod/step-execution-info.model';

/**
 * 工作流执行进度信息
 */
export interface WorkflowExecutionProgress {
  /** executionId */
  executionId: string;
  /** status */
  status: WorkflowExecutionStatus;
  /** totalSteps */
  totalSteps: number;
  /** completedSteps */
  completedSteps: number;
  /** failedSteps */
  failedSteps: number;
  /** progressPercentage */
  progressPercentage: number;
  /** steps */
  steps: StepExecutionInfo[];
  /** currentStepName */
  currentStepName?: string | null;
  /** errorMessage */
  errorMessage?: string | null;
}
