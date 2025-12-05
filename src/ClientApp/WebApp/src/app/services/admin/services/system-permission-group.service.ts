import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemPermissionGroupFilterDto } from '../models/system-mod/system-permission-group-filter-dto.model';
import { PageList } from '../models/ater/page-list.model';
import { SystemPermissionGroupItemDto } from '../models/system-mod/system-permission-group-item-dto.model';
import { SystemPermissionGroupAddDto } from '../models/system-mod/system-permission-group-add-dto.model';
import { SystemPermissionGroup } from '../models/entity/system-permission-group.model';
import { SystemPermissionGroupUpdateDto } from '../models/system-mod/system-permission-group-update-dto.model';
import { SystemPermissionGroupDetailDto } from '../models/system-mod/system-permission-group-detail-dto.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class SystemPermissionGroupService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SystemPermissionGroupFilterDto
   */
  filter(data: SystemPermissionGroupFilterDto): Observable<PageList<SystemPermissionGroupItemDto>> {
    const _url = `/api/SystemPermissionGroup/filter`;
    return this.request<PageList<SystemPermissionGroupItemDto>>('post', _url, data);
  }
  /**
   * 新增 ✅
   * @param data SystemPermissionGroupAddDto
   */
  add(data: SystemPermissionGroupAddDto): Observable<SystemPermissionGroup> {
    const _url = `/api/SystemPermissionGroup`;
    return this.request<SystemPermissionGroup>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemPermissionGroupUpdateDto
   */
  update(id: string, data: SystemPermissionGroupUpdateDto): Observable<boolean> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SystemPermissionGroupDetailDto> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<SystemPermissionGroupDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<boolean>('delete', _url);
  }
}