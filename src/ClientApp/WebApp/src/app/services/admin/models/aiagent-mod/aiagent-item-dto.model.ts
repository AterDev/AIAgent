/**
 * agentItemDto
 */
export interface AIAgentItemDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** applicationId */
  applicationId?: string | null;
  /** is enabled */
  enable?: boolean | null;
  /** isPublic */
  isPublic?: boolean | null;
  /** modelId */
  modelId?: string | null;
  /** Agent 名称 */
  name?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt?: string | null;
}
