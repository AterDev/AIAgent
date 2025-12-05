import { SystemUser } from '../entity/system-user.model';

/**
 * 组织结构
 */
export interface SystemOrganization {
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
  /** 名称 */
  name: string;
  /** 子目录 */
  children: SystemOrganization[];
  /** 组织结构 */
  parent: SystemOrganization;
  /** parentId */
  parentId?: string | null;
  /** users */
  users: SystemUser[];
}
