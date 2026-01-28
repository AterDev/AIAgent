/**
 * 对话实例AddDto
 */
export interface ConversationAddDto {
  /** 对话描述 */
  description?: string | null;
  /** 是否置顶 */
  isPinned: boolean;
  /** 最后活动时间 */
  lastActiveTime: Date;
  /** 使用的AI模型 */
  modelName?: string | null;
  /** 对话名称 */
  name: string;
  /** 系统提示词 */
  systemPrompt?: string | null;
  /** 总令牌数量 */
  totalTokens: number;
  /** 用户ID */
  userId: string;
}
