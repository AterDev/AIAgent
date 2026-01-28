import { AIAgent } from '../entity/aiagent.model';
import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行记录
 */
export interface AgentExecution {
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
  /** agentId */
  agentId: string;
  /** agent */
  agent: AIAgent;
  /** status */
  status: AgentExecutionStatus;
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
