import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationRagCollectionPermissionFilterDto } from '../models/model-mod/application-rag-collection-permission-filter-dto.model';
import { ApplicationRagCollectionPermissionItemDto } from '../models/model-mod/application-rag-collection-permission-item-dto.model';
import { ApplicationRagCollectionPermissionAddDto } from '../models/model-mod/application-rag-collection-permission-add-dto.model';

@Injectable({ providedIn: 'root' })
export class ApplicationRagCollectionPermissionService extends BaseService {
  list(data: ApplicationRagCollectionPermissionFilterDto): Observable<PageList<ApplicationRagCollectionPermissionItemDto>> {
    const _url = `/api/ApplicationRagCollectionPermission/filter`;
    return this.request<PageList<ApplicationRagCollectionPermissionItemDto>>('post', _url, data);
  }

  add(data: ApplicationRagCollectionPermissionAddDto): Observable<any> {
    const _url = `/api/ApplicationRagCollectionPermission`;
    return this.request<any>('post', _url, data);
  }

  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationRagCollectionPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }
}