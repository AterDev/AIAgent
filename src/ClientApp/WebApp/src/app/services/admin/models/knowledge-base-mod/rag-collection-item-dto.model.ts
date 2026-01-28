/**
 * 知识库 ItemDto
 */
export interface RagCollectionItemDto {
  /** id */
  id: string;
  /** name */
  name?: string | null;
  /** isPublic */
  isPublic: boolean;
  /** isEnabled */
  isEnabled: boolean;
}
