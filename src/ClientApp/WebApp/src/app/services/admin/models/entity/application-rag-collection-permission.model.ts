import { Application } from '../entity/application.model';
import { RagCollection } from '../entity/rag-collection.model';

/**
 * 应用知识库关联
 */
export interface ApplicationRagCollectionPermission {
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
  /** applicationId */
  applicationId: string;
  /** 应用定义 */
  application: Application;
  /** ragCollectionId */
  ragCollectionId: string;
  /** 知识库/文档集 */
  ragCollection: RagCollection;
  /** isEnabled */
  isEnabled: boolean;
}
