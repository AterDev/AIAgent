/**
 * 应用定义
 */
export interface Application {
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
  /** isEnabled */
  isEnabled: boolean;
}
