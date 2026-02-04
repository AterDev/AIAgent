/**
 * 提示词FilterDto
 */
export interface AIPromptFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 提示词名称 */
  name?: string | null;
  /** 提示词分组 */
  groupName?: string | null;
}
