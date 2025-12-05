import { SystemPermission } from '../entity/system-permission.model';
import { SystemRole } from '../entity/system-role.model';

/**
 * 系统权限组
 */
export interface SystemPermissionGroup {
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
  /** 权限名称标识 */
  name: string;
  /** 权限说明 */
  description?: string | null;
  /** permissions */
  permissions: SystemPermission[];
  /** roles */
  roles: SystemRole[];
}
