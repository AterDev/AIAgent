import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationRagCollectionPermissionFilterDto } from '../models/model-mod/application-rag-collection-permission-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationRagCollectionPermissionItemDto } from '../models/model-mod/application-rag-collection-permission-item-dto.model';
import { ApplicationRagCollectionPermissionAddDto } from '../models/model-mod/application-rag-collection-permission-add-dto.model';
import { ApplicationRagCollectionPermission } from '../models/entity/application-rag-collection-permission.model';
import { ApplicationRagCollectionPermissionUpdateDto } from '../models/model-mod/application-rag-collection-permission-update-dto.model';
import { ApplicationRagCollectionPermissionDetailDto } from '../models/model-mod/application-rag-collection-permission-detail-dto.model';
/**
 * 应用知识库关联管理
 */
@Injectable({ providedIn: 'root' })
export class ApplicationRagCollectionPermissionService extends BaseService {
  /**
   * list
   * @param data ApplicationRagCollectionPermissionFilterDto
   */
  list(data: ApplicationRagCollectionPermissionFilterDto): Observable<PageList<ApplicationRagCollectionPermissionItemDto>> {
    const _url = `/api/ApplicationRagCollectionPermission/filter`;
    return this.request<PageList<ApplicationRagCollectionPermissionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ApplicationRagCollectionPermissionAddDto
   */
  add(data: ApplicationRagCollectionPermissionAddDto): Observable<ApplicationRagCollectionPermission> {
    const _url = `/api/ApplicationRagCollectionPermission`;
    return this.request<ApplicationRagCollectionPermission>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ApplicationRagCollectionPermissionUpdateDto
   */
  update(id: string, data: ApplicationRagCollectionPermissionUpdateDto): Observable<boolean> {
    const _url = `/api/ApplicationRagCollectionPermission/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ApplicationRagCollectionPermissionDetailDto> {
    const _url = `/api/ApplicationRagCollectionPermission/${id}`;
    return this.request<ApplicationRagCollectionPermissionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationRagCollectionPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }
}