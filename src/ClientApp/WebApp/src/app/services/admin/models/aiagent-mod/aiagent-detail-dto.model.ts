/**
 * agentDetailDto
 */
export interface AIAgentDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** applicationId */
  applicationId?: string | null;
  /** Agent 描述信息 */
  description?: string | null;
  /** is enabled */
  enable?: boolean | null;
  /** isPublic */
  isPublic?: boolean | null;
  /** Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"） */
  modelId?: string | null;
  /** Agent 名称 */
  name?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt?: string | null;
  /** tenantId */
  tenantId: string;
  /** Agent 可用的工具列表 */
  tools?: string[] | null;
  /** updatedTime */
  updatedTime: Date;
}
