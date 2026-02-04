import { Workflow } from '../entity/workflow.model';
import { WorkflowExecutionStatus } from '../entity/workflow-execution-status.model';
import { WorkflowExecutionMode } from '../entity/workflow-execution-mode.model';

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
  /** executionMode */
  executionMode: WorkflowExecutionMode;
  /** inputJson */
  inputJson: string;
  /** outputJson */
  outputJson: string;
  /** 全局执行上下文（所有步骤的中间结果） */
  contextJson: string;
  /** 步骤执行记录（序列化的 StepExecution 列表） */
  stepExecutionsJson?: string | null;
  /** 上一次检查点（用于断点续传） */
  lastCheckpointStepIndex?: number | null;
  /** 已执行步骤数量 */
  executedStepCount: number;
  /** 重试次数 */
  retryCount: number;
  /** 最大重试次数 */
  maxRetries: number;
  /** 是否已放弃 */
  isAbandoned: boolean;
  /** completedTime */
  completedTime?: Date | null;
  /** durationMs */
  durationMs: number;
  /** errorMessage */
  errorMessage?: string | null;
  /** 断点续传的恢复时间 */
  resumedAt?: Date | null;
}
