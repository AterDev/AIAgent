import { StepExecutionStatus } from '../entity/step-execution-status.model';

/**
 * 步骤执行信息
 */
export interface StepExecutionInfo {
  /** index */
  index: number;
  /** name */
  name: string;
  /** status */
  status: StepExecutionStatus;
  /** durationMs */
  durationMs: number;
  /** errorMessage */
  errorMessage?: string | null;
}
