/**
 * RAG 模型配置FilterDto
 */
export interface RagAgentConfigFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 配置项名称 */
  key?: string | null;
}
