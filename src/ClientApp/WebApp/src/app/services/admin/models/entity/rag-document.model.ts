import { RagCollection } from '../entity/rag-collection.model';
import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档
 */
export interface RagDocument {
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
  /** collectionId */
  collectionId: string;
  /** 知识库/文档集 */
  collection: RagCollection;
  /** name */
  name: string;
  /** fileName */
  fileName: string;
  /** contentType */
  contentType: string;
  /** status */
  status: RagDocumentStatus;
  /** tags */
  tags: string[];
  /** roles */
  roles: string[];
  /** sourceUrl */
  sourceUrl?: string | null;
  /** chunkCount */
  chunkCount: number;
  /** tokenCount */
  tokenCount: number;
  /** errorMessage */
  errorMessage?: string | null;
}
