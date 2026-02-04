import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationToolPermissionFilterDto } from '../models/model-mod/application-tool-permission-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationToolPermissionItemDto } from '../models/model-mod/application-tool-permission-item-dto.model';
import { ApplicationToolPermissionAddDto } from '../models/model-mod/application-tool-permission-add-dto.model';
import { ApplicationToolPermission } from '../models/entity/application-tool-permission.model';
import { ApplicationToolPermissionUpdateDto } from '../models/model-mod/application-tool-permission-update-dto.model';
import { ApplicationToolPermissionDetailDto } from '../models/model-mod/application-tool-permission-detail-dto.model';
/**
 * 应用工具权限管理
 */
@Injectable({ providedIn: 'root' })
export class ApplicationToolPermissionService extends BaseService {
  /**
   * list
   * @param data ApplicationToolPermissionFilterDto
   */
  list(data: ApplicationToolPermissionFilterDto): Observable<PageList<ApplicationToolPermissionItemDto>> {
    const _url = `/api/ApplicationToolPermission/filter`;
    return this.request<PageList<ApplicationToolPermissionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ApplicationToolPermissionAddDto
   */
  add(data: ApplicationToolPermissionAddDto): Observable<ApplicationToolPermission> {
    const _url = `/api/ApplicationToolPermission`;
    return this.request<ApplicationToolPermission>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ApplicationToolPermissionUpdateDto
   */
  update(id: string, data: ApplicationToolPermissionUpdateDto): Observable<boolean> {
    const _url = `/api/ApplicationToolPermission/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ApplicationToolPermissionDetailDto> {
    const _url = `/api/ApplicationToolPermission/${id}`;
    return this.request<ApplicationToolPermissionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationToolPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }
}