/**
 * 对话实例ItemDto
 */
export interface ConversationItemDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** 是否置顶 */
  isPinned?: boolean | null;
  /** 最后活动时间 */
  lastActiveTime?: Date | null;
  /** 使用的AI模型 */
  modelName?: string | null;
  /** 系统提示词 */
  systemPrompt?: string | null;
  /** 总令牌数量 */
  totalTokens?: number | null;
}
