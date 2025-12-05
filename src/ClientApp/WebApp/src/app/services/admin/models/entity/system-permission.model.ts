import { PermissionType } from '../entity/permission-type.model';
import { SystemPermissionGroup } from '../entity/system-permission-group.model';

/**
 * 权限
 */
export interface SystemPermission {
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
  /** 是否启用 */
  enable: boolean;
  /** 权限类型 */
  permissionType: PermissionType;
  /** 系统权限组 */
  group: SystemPermissionGroup;
  /** groupId */
  groupId: string;
}
