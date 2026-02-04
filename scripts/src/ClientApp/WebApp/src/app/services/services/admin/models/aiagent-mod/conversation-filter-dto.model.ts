/**
 * 对话实例FilterDto
 */
export interface ConversationFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 是否置顶 */
  isPinned?: boolean | null;
  /** 用户ID */
  userId?: string | null;
}
