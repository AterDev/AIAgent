/**
 * 分块 ItemDto
 */
export interface RagChunkItemDto {
  /** id */
  id: string;
  /** documentId */
  documentId: string;
  /** chunkIndex */
  chunkIndex: number;
  /** tokenCount */
  tokenCount: number;
}
