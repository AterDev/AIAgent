/**
 * Agent 执行请求
 */
export interface AgentExecuteRequestDto {
  /** applicationId */
  applicationId: string;
  /** inputJson */
  inputJson?: string | null;
}
