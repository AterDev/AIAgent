/**
 * 知识库 UpdateDto
 */
export interface RagCollectionUpdateDto {
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** isPublic */
  isPublic?: boolean | null;
  /** isEnabled */
  isEnabled?: boolean | null;
  /** tags */
  tags?: string[] | null;
}
