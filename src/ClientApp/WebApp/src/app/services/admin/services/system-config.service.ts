import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemConfigFilterDto } from '../models/system-mod/system-config-filter-dto.model';
import { PageList } from '../models/ater/page-list.model';
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
   * 获取配置列表 ✅
   * @param data SystemConfigFilterDto
   */
  filter(data: SystemConfigFilterDto): Observable<PageList<SystemConfigItemDto>> {
    const _url = `/api/SystemConfig/filter`;
    return this.request<PageList<SystemConfigItemDto>>('post', _url, data);
  }
  /**
   * 获取枚举信息 ✅
   */
  getEnumConfigs(): Observable<Record<string, any[]>> {
    const _url = `/api/SystemConfig/enum`;
    return this.request<Record<string, any[]>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SystemConfigAddDto
   */
  add(data: SystemConfigAddDto): Observable<SystemConfig> {
    const _url = `/api/SystemConfig`;
    return this.request<SystemConfig>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemConfigUpdateDto
   */
  update(id: string, data: SystemConfigUpdateDto): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SystemConfigDetailDto> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<SystemConfigDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('delete', _url);
  }
}