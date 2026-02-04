/**
 * 对话实例UpdateDto
 */
export interface ConversationUpdateDto {
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
  /** 总令牌数量 */
  totalTokens?: number | null;
  /** 用户ID */
  userId?: string | null;
}
