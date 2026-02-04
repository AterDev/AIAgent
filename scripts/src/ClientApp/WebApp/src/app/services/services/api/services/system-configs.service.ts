import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemConfigFilterDto } from '../models/system-mod/system-config-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SystemConfigItemDto } from '../models/system-mod/system-config-item-dto.model';
import { SystemConfigDetailDto } from '../models/system-mod/system-config-detail-dto.model';
/**
 * Open platform system configs
 */
@Injectable({ providedIn: 'root' })
export class SystemConfigsService extends BaseService {
  /**
   * list
   * @param data SystemConfigFilterDto
   */
  list(data: SystemConfigFilterDto): Observable<PageList<SystemConfigItemDto>> {
    const _url = `/api/v1/system-configs/filter`;
    return this.request<PageList<SystemConfigItemDto>>('post', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<SystemConfigDetailDto> {
    const _url = `/api/v1/system-configs/${id}`;
    return this.request<SystemConfigDetailDto>('get', _url);
  }
}