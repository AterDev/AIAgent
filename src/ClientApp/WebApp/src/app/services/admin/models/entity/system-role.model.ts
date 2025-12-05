import { SystemUser } from '../entity/system-user.model';
import { SystemPermissionGroup } from '../entity/system-permission-group.model';
import { SystemMenu } from '../entity/system-menu.model';

/**
 * 系统角色
 */
export interface SystemRole {
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
  /** 角色名称 */
  name: string;
  /** 角色标识 */
  nameValue: string;
  /** 是否系统内置 */
  isSystem: boolean;
  /** 图标 */
  icon?: string | null;
  /** users */
  users: SystemUser[];
  /** 中间表 */
  permissionGroups: SystemPermissionGroup[];
  /** 菜单权限 */
  systemMenus: SystemMenu[];
}
