import { AgentExecutionStatus } from '../entity/agent-execution-status.model';

/**
 * Agent 执行 AddDto
 */
export interface AgentExecutionAddDto {
  /** agentId */
  agentId: string;
  /** inputJson */
  inputJson?: string | null;
  /** status */
  status: AgentExecutionStatus;
}
