import { RagDocumentStatus } from '../entity/rag-document-status.model';
import { StorageType } from '../entity/storage-type.model';

/**
 * 文档 UpdateDto
 */
export interface RagDocumentUpdateDto {
  /** name */
  name?: string | null;
  /** fileName */
  fileName?: string | null;
  /** filePath */
  filePath?: string | null;
  /** contentType */
  contentType?: string | null;
  /** storageType */
  storageType?: StorageType | null;
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
