import { ChatMessageType } from '../entity/chat-message-type.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';

/**
 * 聊天消息FilterDto
 */
export interface ChatMessageFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** contentType */
  contentType?: ChatMessageType | null;
  /** role */
  role?: ChatMessageRole | null;
  /** 用户ID */
  userId?: string | null;
  /** conversationId */
  conversationId?: string | null;
}
