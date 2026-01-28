import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行 FilterDto
 */
export interface AgentExecutionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** agentId */
  agentId?: string | null;
  /** status */
  status?: AgentExecutionStatus | null;
}
