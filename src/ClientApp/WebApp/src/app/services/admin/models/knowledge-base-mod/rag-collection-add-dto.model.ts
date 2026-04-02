/**
 * 知识库 AddDto
 */
export interface RagCollectionAddDto {
  /** applicationId */
  applicationId?: string | null;
  /** name */
  name: string;
  /** description */
  description?: string | null;
  /** isPublic */
  isPublic: boolean;
  /** isEnabled */
  isEnabled: boolean;
  /** tags */
  tags?: string[] | null;
}
