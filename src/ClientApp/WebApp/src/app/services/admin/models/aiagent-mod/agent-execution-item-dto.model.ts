import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行 ItemDto
 */
export interface AgentExecutionItemDto {
  /** id */
  id: string;
  /** agentId */
  agentId: string;
  /** status */
  status: AgentExecutionStatus;
  /** durationMs */
  durationMs: number;
}
