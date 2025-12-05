import { SystemPermission } from '../entity/system-permission.model';

export interface SystemPermissionGroupItemDto {
  /** id */
  id: string;
  /** 权限名称标识 */
  name: string;
  /** 权限说明 */
  description?: string | null;
  /** permissions */
  permissions?: SystemPermission[] | null;
}
