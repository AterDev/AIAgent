import { RagDocumentStatus } from '../entity/rag-document-status.model';
import { StorageType } from '../entity/storage-type.model';

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
  /** filePath */
  filePath?: string | null;
  /** contentType */
  contentType?: string | null;
  /** storageType */
  storageType?: StorageType | null;
  /** status */
  status: RagDocumentStatus;
  /** tags */
  tags?: string[] | null;
  /** roles */
  roles?: string[] | null;
  /** sourceUrl */
  sourceUrl?: string | null;
}
