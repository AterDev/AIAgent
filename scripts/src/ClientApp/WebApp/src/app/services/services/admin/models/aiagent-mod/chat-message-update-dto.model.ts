import { ChatMessageType } from '../entity/chat-message-type.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';

/**
 * 聊天消息UpdateDto
 */
export interface ChatMessageUpdateDto {
  /** 消息内容 */
  content?: string | null;
  /** contentType */
  contentType?: ChatMessageType | null;
  /** 模型名称 */
  modelName?: string | null;
  /** role */
  role?: ChatMessageRole | null;
  /** 令牌数量 */
  tokenCount?: number | null;
  /** 用户ID */
  userId?: string | null;
  /** conversationId */
  conversationId?: string | null;
}
