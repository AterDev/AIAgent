import { RagDocumentStatus } from '../entity/rag-document-status.model';

/**
 * 文档 DetailDto
 */
export interface RagDocumentDetailDto {
  /** id */
  id: string;
  /** collectionId */
  collectionId: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
  /** fileName */
  fileName?: string | null;
  /** filePath */
  filePath?: string | null;
  /** fileType */
  fileType?: string | null;
  /** 存储服务商ID */
  storageProviderId: string;
  /** status */
  status: RagDocumentStatus;
  /** tags */
  tags?: string[] | null;
  /** roles */
  roles?: string[] | null;
  /** chunkCount */
  chunkCount: number;
  /** tokenCount */
  tokenCount: number;
  /** errorMessage */
  errorMessage?: string | null;
}
