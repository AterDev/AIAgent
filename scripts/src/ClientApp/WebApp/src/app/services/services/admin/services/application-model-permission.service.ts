import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationModelPermissionFilterDto } from '../models/model-mod/application-model-permission-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationModelPermissionItemDto } from '../models/model-mod/application-model-permission-item-dto.model';
import { ApplicationModelPermissionAddDto } from '../models/model-mod/application-model-permission-add-dto.model';
import { ApplicationModelPermission } from '../models/entity/application-model-permission.model';
import { ApplicationModelPermissionUpdateDto } from '../models/model-mod/application-model-permission-update-dto.model';
import { ApplicationModelPermissionDetailDto } from '../models/model-mod/application-model-permission-detail-dto.model';
/**
 * 应用模型权限管理
 */
@Injectable({ providedIn: 'root' })
export class ApplicationModelPermissionService extends BaseService {
  /**
   * list
   * @param data ApplicationModelPermissionFilterDto
   */
  list(data: ApplicationModelPermissionFilterDto): Observable<PageList<ApplicationModelPermissionItemDto>> {
    const _url = `/api/ApplicationModelPermission/filter`;
    return this.request<PageList<ApplicationModelPermissionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ApplicationModelPermissionAddDto
   */
  add(data: ApplicationModelPermissionAddDto): Observable<ApplicationModelPermission> {
    const _url = `/api/ApplicationModelPermission`;
    return this.request<ApplicationModelPermission>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ApplicationModelPermissionUpdateDto
   */
  update(id: string, data: ApplicationModelPermissionUpdateDto): Observable<boolean> {
    const _url = `/api/ApplicationModelPermission/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ApplicationModelPermissionDetailDto> {
    const _url = `/api/ApplicationModelPermission/${id}`;
    return this.request<ApplicationModelPermissionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationModelPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }
}