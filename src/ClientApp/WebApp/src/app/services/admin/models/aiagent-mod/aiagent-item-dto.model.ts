/**
 * agentItemDto
 */
export interface AIAgentItemDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** is enabled */
  enable?: boolean | null;
  /** isTemplate */
  isTemplate?: boolean | null;
  /** Agent 名称 */
  name?: string | null;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt?: string | null;
}
