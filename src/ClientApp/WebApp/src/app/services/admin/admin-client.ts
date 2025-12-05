import { inject, Injectable } from '@angular/core';
import { ArticleCategoryService } from './services/article-category.service';
import { SystemConfigService } from './services/system-config.service';
import { SystemLogsService } from './services/system-logs.service';
import { SystemMenuService } from './services/system-menu.service';
import { SystemPermissionService } from './services/system-permission.service';
import { SystemPermissionGroupService } from './services/system-permission-group.service';
import { SystemRoleService } from './services/system-role.service';
import { SystemUserService } from './services/system-user.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  /** ArticleCategory */
  public articleCategory = inject(ArticleCategoryService);
  /** 系统配置 */
  public systemConfig = inject(SystemConfigService);
  /** 系统日志 */
  public systemLogs = inject(SystemLogsService);
  /** 系统菜单 */
  public systemMenu = inject(SystemMenuService);
  /** 权限 */
  public systemPermission = inject(SystemPermissionService);
  /** SystemPermissionGroup */
  public systemPermissionGroup = inject(SystemPermissionGroupService);
  /** 系统角色
SystemMod.Managers.SystemRoleManager */
  public systemRole = inject(SystemRoleService);
  /** 系统用户 */
  public systemUser = inject(SystemUserService);
}
