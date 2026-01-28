import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档 UpdateDto
 */
export interface RagDocumentUpdateDto {
  /** name */
  name?: string | null;
  /** fileName */
  fileName?: string | null;
  /** contentType */
  contentType?: string | null;
  /** status */
  status?: RagDocumentStatus | null;
  /** tags */
  tags?: string[] | null;
  /** roles */
  roles?: string[] | null;
  /** sourceUrl */
  sourceUrl?: string | null;
  /** chunkCount */
  chunkCount?: number | null;
  /** tokenCount */
  tokenCount?: number | null;
  /** errorMessage */
  errorMessage?: string | null;
}
