import { ChatMessageType } from '../entity/chat-message-type.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';

/**
 * 聊天消息AddDto
 */
export interface ChatMessageAddDto {
  /** 消息内容 */
  content: string;
  /** contentType */
  contentType: ChatMessageType;
  /** 模型名称 */
  modelName?: string | null;
  /** role */
  role: ChatMessageRole;
  /** 令牌数量 */
  tokenCount?: number | null;
  /** 用户ID */
  userId: string;
  /** conversationId */
  conversationId: string;
}
