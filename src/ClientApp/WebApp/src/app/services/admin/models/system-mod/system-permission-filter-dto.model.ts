import { PermissionType } from '../entity/permission-type.model';

/**
 * 权限查询筛选
 */
export interface SystemPermissionFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 权限名称标识 */
  name?: string | null;
  /** 权限类型 */
  permissionType?: PermissionType | null;
  /** groupId */
  groupId?: string | null;
}
