import { ChatMessageType } from '../entity/chat-message-type.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';

/**
 * 聊天消息ItemDto
 */
export interface ChatMessageItemDto {
  /** id */
  id: string;
  /** 消息内容 */
  content?: string | null;
  /** contentType */
  contentType: ChatMessageType;
  /** createdTime */
  createdTime: Date;
  /** 模型名称 */
  modelName?: string | null;
  /** role */
  role: ChatMessageRole;
  /** 令牌数量 */
  tokenCount?: number | null;
}
