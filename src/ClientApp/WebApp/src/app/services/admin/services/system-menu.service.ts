import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemMenuFilterDto } from '../models/system-mod/system-menu-filter-dto.model';
import { PageList } from '../models/ater/page-list.model';
import { SystemMenu } from '../models/entity/system-menu.model';
import { SystemMenuAddDto } from '../models/system-mod/system-menu-add-dto.model';
import { SystemMenuUpdateDto } from '../models/system-mod/system-menu-update-dto.model';
/**
 * 系统菜单
 */
@Injectable({ providedIn: 'root' })
export class SystemMenuService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SystemMenuFilterDto
   */
  filter(data: SystemMenuFilterDto): Observable<PageList<SystemMenu>> {
    const _url = `/api/SystemMenu/filter`;
    return this.request<PageList<SystemMenu>>('post', _url, data);
  }
  /**
   * 新增 ✅
   * @param data SystemMenuAddDto
   */
  add(data: SystemMenuAddDto): Observable<SystemMenu> {
    const _url = `/api/SystemMenu`;
    return this.request<SystemMenu>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemMenuUpdateDto
   */
  update(id: string, data: SystemMenuUpdateDto): Observable<boolean> {
    const _url = `/api/SystemMenu/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SystemMenu> {
    const _url = `/api/SystemMenu/${id}`;
    return this.request<SystemMenu>('get', _url);
  }
}