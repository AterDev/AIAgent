import { RagDocument } from '../entity/rag-document.model';

/**
 * 文档分块
 */
export interface RagChunk {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** documentId */
  documentId: string;
  /** 文档 */
  document: RagDocument;
  /** chunkIndex */
  chunkIndex: number;
  /** content */
  content: string;
  /** tokenCount */
  tokenCount: number;
  /** vectorId */
  vectorId?: string | null;
}
