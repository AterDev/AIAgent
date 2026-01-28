/**
 * 工作流 FilterDto
 */
export interface WorkflowFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** isPublished */
  isPublished?: boolean | null;
}
