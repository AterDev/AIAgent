/**
 * 工作流定义
 */
export interface Workflow {
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
  /** definitionJson */
  definitionJson: string;
  /** version */
  version: number;
  /** isPublished */
  isPublished: boolean;
}
