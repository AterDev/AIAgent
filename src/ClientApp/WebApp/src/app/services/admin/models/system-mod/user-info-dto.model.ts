import { SystemMenu } from '../entity/system-menu.model';
import { SystemPermissionGroup } from '../entity/system-permission-group.model';

export interface UserInfoDto {
  /** id */
  id: string;
  /** username */
  username: string;
  /** roles */
  roles: string[];
  /** menus */
  menus?: SystemMenu[] | null;
  /** permissionGroups */
  permissionGroups?: SystemPermissionGroup[] | null;
}
