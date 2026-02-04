/**
 * 知识库/文档集
 */
export interface RagCollection {
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
  /** name */
  name: string;
  /** description */
  description: string;
  /** isPublic */
  isPublic: boolean;
  /** isEnabled */
  isEnabled: boolean;
  /** tags */
  tags: string[];
}
