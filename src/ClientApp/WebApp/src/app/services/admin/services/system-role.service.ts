import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/ater/page-list.model';
import { SystemRoleItemDto } from '../models/system-mod/system-role-item-dto.model';
import { SystemRoleAddDto } from '../models/system-mod/system-role-add-dto.model';
import { SystemRole } from '../models/entity/system-role.model';
import { SystemRoleUpdateDto } from '../models/system-mod/system-role-update-dto.model';
import { SystemRoleDetailDto } from '../models/system-mod/system-role-detail-dto.model';
import { SystemRoleSetMenusDto } from '../models/system-mod/system-role-set-menus-dto.model';
import { SystemRoleSetPermissionGroupsDto } from '../models/system-mod/system-role-set-permission-groups-dto.model';
/**
 * 系统角色
SystemMod.Managers.SystemRoleManager
 */
@Injectable({ providedIn: 'root' })
export class SystemRoleService extends BaseService {
  /**
   * 筛选 ✅
   * @param name 角色显示名称
   * @param nameValue 角色名，系统标识
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(name: string | null, nameValue: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<SystemRoleItemDto>> {
    const _url = `/api/SystemRole?name=${name ?? ''}&nameValue=${nameValue ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<SystemRoleItemDto>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SystemRoleAddDto
   */
  add(data: SystemRoleAddDto): Observable<SystemRole> {
    const _url = `/api/SystemRole`;
    return this.request<SystemRole>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemRoleUpdateDto
   */
  update(id: string, data: SystemRoleUpdateDto): Observable<SystemRole> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<SystemRole>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  detail(id: string): Observable<SystemRoleDetailDto> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<SystemRoleDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<any> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * 角色菜单 ✅
   * @param data SystemRoleSetMenusDto
   */
  updateMenus(data: SystemRoleSetMenusDto): Observable<SystemRole> {
    const _url = `/api/SystemRole/menus`;
    return this.request<SystemRole>('put', _url, data);
  }
  /**
   * Set Permission Group ✅
   * @param data SystemRoleSetPermissionGroupsDto
   */
  updatePermissionGroups(data: SystemRoleSetPermissionGroupsDto): Observable<SystemRole> {
    const _url = `/api/SystemRole/permissionGroups`;
    return this.request<SystemRole>('put', _url, data);
  }
}