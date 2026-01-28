import { ChatMessageType } from '../entity/chat-message-type.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';

/**
 * 聊天消息DetailDto
 */
export interface ChatMessageDetailDto {
  /** id */
  id: string;
  /** 消息内容 */
  content?: string | null;
  /** contentType */
  contentType: ChatMessageType;
  /** 对话ID */
  conversationId?: string | null;
  /** createdTime */
  createdTime: Date;
  /** 模型名称 */
  modelName?: string | null;
  /** role */
  role: ChatMessageRole;
  /** tenantId */
  tenantId: string;
  /** 令牌数量 */
  tokenCount?: number | null;
  /** updatedTime */
  updatedTime: Date;
  /** 用户ID */
  userId?: string | null;
}
