/**
 * 分块 UpdateDto
 */
export interface RagChunkUpdateDto {
  /** chunkIndex */
  chunkIndex?: number | null;
  /** content */
  content?: string | null;
  /** tokenCount */
  tokenCount?: number | null;
  /** vectorId */
  vectorId?: string | null;
}
