/**
 * 对话实例DetailDto
 */
export interface ConversationDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** 对话描述 */
  description?: string | null;
  /** 是否置顶 */
  isPinned?: boolean | null;
  /** 最后活动时间 */
  lastActiveTime?: Date | null;
  /** 使用的AI模型 */
  modelName?: string | null;
  /** 对话名称 */
  name?: string | null;
  /** 系统提示词 */
  systemPrompt?: string | null;
  /** tenantId */
  tenantId: string;
  /** 总令牌数量 */
  totalTokens?: number | null;
  /** updatedTime */
  updatedTime: Date;
  /** 用户ID */
  userId?: string | null;
}
