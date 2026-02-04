/**
 * 知识库 DetailDto
 */
export interface RagCollectionDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** isPublic */
  isPublic: boolean;
  /** isEnabled */
  isEnabled: boolean;
  /** tags */
  tags?: string[] | null;
}
