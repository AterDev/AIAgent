import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemConfigFilterDto } from '../models/system-mod/system-config-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SystemConfigItemDto } from '../models/system-mod/system-config-item-dto.model';
import { SystemConfigAddDto } from '../models/system-mod/system-config-add-dto.model';
import { SystemConfig } from '../models/entity/system-config.model';
import { SystemConfigUpdateDto } from '../models/system-mod/system-config-update-dto.model';
import { SystemConfigDetailDto } from '../models/system-mod/system-config-detail-dto.model';
/**
 * 系统配置
 */
@Injectable({ providedIn: 'root' })
export class SystemConfigService extends BaseService {
  /**
   * list 系统配置 with page ✍️
   * @param data SystemConfigFilterDto
   */
  list(data: SystemConfigFilterDto): Observable<PageList<SystemConfigItemDto>> {
    const _url = `/api/SystemConfig/filter`;
    return this.request<PageList<SystemConfigItemDto>>('post', _url, data);
  }
  /**
   * Add 系统配置 ✍️
   * @param data SystemConfigAddDto
   */
  add(data: SystemConfigAddDto): Observable<SystemConfig> {
    const _url = `/api/SystemConfig`;
    return this.request<SystemConfig>('post', _url, data);
  }
  /**
   * Update 系统配置 ✍️
   * @param id
   * @param data SystemConfigUpdateDto
   */
  update(id: string, data: SystemConfigUpdateDto): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 系统配置 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<SystemConfigDetailDto> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<SystemConfigDetailDto>('get', _url);
  }
  /**
   * Delete 系统配置 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('delete', _url);
  }
}