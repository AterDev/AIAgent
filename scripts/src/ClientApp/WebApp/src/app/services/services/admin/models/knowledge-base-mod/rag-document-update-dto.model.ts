import { RagDocumentStatus } from '../entity/rag-document-status.model';

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
  /** fileType */
  fileType?: string | null;
  /** 存储服务商ID */
  storageProviderId?: string | null;
  /** status */
  status?: RagDocumentStatus | null;
  /** tags */
  tags?: string[] | null;
  /** roles */
  roles?: string[] | null;
  /** chunkCount */
  chunkCount?: number | null;
  /** tokenCount */
  tokenCount?: number | null;
  /** errorMessage */
  errorMessage?: string | null;
}
