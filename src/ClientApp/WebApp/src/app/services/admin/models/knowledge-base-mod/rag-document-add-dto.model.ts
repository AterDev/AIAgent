import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档 AddDto
 */
export interface RagDocumentAddDto {
  /** collectionId */
  collectionId: string;
  /** name */
  name: string;
  /** fileName */
  fileName?: string | null;
  /** contentType */
  contentType?: string | null;
  /** status */
  status: RagDocumentStatus;
  /** tags */
  tags?: string[] | null;
  /** roles */
  roles?: string[] | null;
  /** sourceUrl */
  sourceUrl?: string | null;
}
