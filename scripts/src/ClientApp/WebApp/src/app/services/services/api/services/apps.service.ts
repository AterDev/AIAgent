import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationFilterDto } from '../models/model-mod/application-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationItemDto } from '../models/model-mod/application-item-dto.model';
import { ApplicationDetailDto } from '../models/model-mod/application-detail-dto.model';
/**
 * Open platform apps
 */
@Injectable({ providedIn: 'root' })
export class AppsService extends BaseService {
  /**
   * list
   * @param data ApplicationFilterDto
   */
  list(data: ApplicationFilterDto): Observable<PageList<ApplicationItemDto>> {
    const _url = `/api/v1/apps/filter`;
    return this.request<PageList<ApplicationItemDto>>('post', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ApplicationDetailDto> {
    const _url = `/api/v1/apps/${id}`;
    return this.request<ApplicationDetailDto>('get', _url);
  }
}