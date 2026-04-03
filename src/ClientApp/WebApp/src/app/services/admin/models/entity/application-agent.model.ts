/**
 * application agent
 */
export interface ApplicationAgent {
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
  /** Agent 名称 */
  name: string;
  /** Agent 描述信息 */
  description: string;
  /** Agent 所使用的大模型名称 */
  modelId: string;
  /** Agent 的角色设定（System Prompt） */
  systemPrompt: string;
  /** Agent 可用的工具列表 */
  tools: string[];
  /** is enabled */
  enable: boolean;
  /** applicationId */
  applicationId: string;
  /** userId */
  userId?: string | null;
}