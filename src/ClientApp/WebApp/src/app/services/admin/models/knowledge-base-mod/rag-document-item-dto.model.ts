import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档 ItemDto
 */
export interface RagDocumentItemDto {
  /** id */
  id: string;
  /** collectionId */
  collectionId: string;
  /** name */
  name?: string | null;
  /** status */
  status: RagDocumentStatus;
  /** chunkCount */
  chunkCount: number;
}
