import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemPermissionFilterDto } from '../models/system-mod/system-permission-filter-dto.model';
import { PageList } from '../models/ater/page-list.model';
import { SystemPermissionItemDto } from '../models/system-mod/system-permission-item-dto.model';
import { SystemPermissionAddDto } from '../models/system-mod/system-permission-add-dto.model';
import { SystemPermission } from '../models/entity/system-permission.model';
import { SystemPermissionUpdateDto } from '../models/system-mod/system-permission-update-dto.model';
import { SystemPermissionDetailDto } from '../models/system-mod/system-permission-detail-dto.model';
/**
 * 权限
 */
@Injectable({ providedIn: 'root' })
export class SystemPermissionService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SystemPermissionFilterDto
   */
  filter(data: SystemPermissionFilterDto): Observable<PageList<SystemPermissionItemDto>> {
    const _url = `/api/SystemPermission/filter`;
    return this.request<PageList<SystemPermissionItemDto>>('post', _url, data);
  }
  /**
   * 新增 ✅
   * @param data SystemPermissionAddDto
   */
  add(data: SystemPermissionAddDto): Observable<SystemPermission> {
    const _url = `/api/SystemPermission`;
    return this.request<SystemPermission>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemPermissionUpdateDto
   */
  update(id: string, data: SystemPermissionUpdateDto): Observable<boolean> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SystemPermissionDetailDto> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<SystemPermissionDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }
}