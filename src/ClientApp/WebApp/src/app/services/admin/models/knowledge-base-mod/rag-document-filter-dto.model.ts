import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档 FilterDto
 */
export interface RagDocumentFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** collectionId */
  collectionId?: string | null;
  /** name */
  name?: string | null;
  /** status */
  status?: RagDocumentStatus | null;
}
