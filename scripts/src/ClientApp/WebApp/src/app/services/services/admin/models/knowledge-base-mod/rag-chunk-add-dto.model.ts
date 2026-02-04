/**
 * 分块 AddDto
 */
export interface RagChunkAddDto {
  /** documentId */
  documentId: string;
  /** chunkIndex */
  chunkIndex: number;
  /** content */
  content: string;
  /** tokenCount */
  tokenCount: number;
  /** vectorId */
  vectorId?: string | null;
}
