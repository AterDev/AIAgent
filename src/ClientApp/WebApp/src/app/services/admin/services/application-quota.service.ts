import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationQuotaFilterDto } from '../models/model-mod/application-quota-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationQuotaItemDto } from '../models/model-mod/application-quota-item-dto.model';
import { ApplicationQuotaAddDto } from '../models/model-mod/application-quota-add-dto.model';
import { ApplicationQuota } from '../models/entity/application-quota.model';
import { ApplicationQuotaUpdateDto } from '../models/model-mod/application-quota-update-dto.model';
import { ApplicationQuotaDetailDto } from '../models/model-mod/application-quota-detail-dto.model';
/**
 * 应用配额管理
 */
@Injectable({ providedIn: 'root' })
export class ApplicationQuotaService extends BaseService {
  /**
   * list
   * @param data ApplicationQuotaFilterDto
   */
  list(data: ApplicationQuotaFilterDto): Observable<PageList<ApplicationQuotaItemDto>> {
    const _url = `/api/ApplicationQuota/filter`;
    return this.request<PageList<ApplicationQuotaItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ApplicationQuotaAddDto
   */
  add(data: ApplicationQuotaAddDto): Observable<ApplicationQuota> {
    const _url = `/api/ApplicationQuota`;
    return this.request<ApplicationQuota>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ApplicationQuotaUpdateDto
   */
  update(id: string, data: ApplicationQuotaUpdateDto): Observable<boolean> {
    const _url = `/api/ApplicationQuota/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ApplicationQuotaDetailDto> {
    const _url = `/api/ApplicationQuota/${id}`;
    return this.request<ApplicationQuotaDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationQuota/${id}`;
    return this.request<boolean>('delete', _url);
  }
}