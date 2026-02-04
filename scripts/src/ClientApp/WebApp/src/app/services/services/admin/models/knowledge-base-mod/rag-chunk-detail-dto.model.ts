/**
 * 分块 DetailDto
 */
export interface RagChunkDetailDto {
  /** id */
  id: string;
  /** documentId */
  documentId: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** chunkIndex */
  chunkIndex: number;
  /** content */
  content?: string | null;
  /** tokenCount */
  tokenCount: number;
  /** vectorId */
  vectorId?: string | null;
}
