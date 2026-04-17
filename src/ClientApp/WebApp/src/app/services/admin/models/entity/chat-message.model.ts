import { Conversation } from '../entity/conversation.model';
import { ChatMessageRole } from '../entity/chat-message-role.model';
import { ChatMessageType } from '../entity/chat-message-type.model';

/**
 * 聊天消息
 */
export interface ChatMessage {
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
  /** 对话ID */
  conversationId: string;
  /** 对话实例 */
  conversation: Conversation;
  /** role */
  role: ChatMessageRole;
  /** 消息内容 */
  content: string;
  /** contentType */
  contentType: ChatMessageType;
  /** 令牌数量 */
  tokenCount?: number | null;
  /** 模型名称 */
  modelName?: string | null;
  /** 附件 URL（持久化层仅保存远程/对象存储 URL） */
  attachmentUrl?: string | null;
  /** 附件 MIME 类型（如 image/png, application/pdf） */
  attachmentMime?: string | null;
  /** 附件原始文件名 */
  attachmentName?: string | null;
}
