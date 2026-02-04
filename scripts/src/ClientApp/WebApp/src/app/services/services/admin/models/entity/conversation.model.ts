import { ChatMessage } from '../entity/chat-message.model';

/**
 * 对话实例
 */
export interface Conversation {
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
  /** 用户ID */
  userId: string;
  /** 对话名称 */
  name: string;
  /** 对话描述 */
  description?: string | null;
  /** 使用的AI模型 */
  modelName?: string | null;
  /** 系统提示词 */
  systemPrompt?: string | null;
  /** 总令牌数量 */
  totalTokens: number;
  /** 是否置顶 */
  isPinned: boolean;
  /** 最后活动时间 */
  lastActiveTime: Date;
  /** 对话中的消息列表 */
  messages: ChatMessage[];
}
