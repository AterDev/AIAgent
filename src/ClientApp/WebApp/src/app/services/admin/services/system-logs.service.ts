import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemLogsFilterDto } from '../models/system-mod/system-logs-filter-dto.model';
import { PageList } from '../models/ater/page-list.model';
import { SystemLogsItemDto } from '../models/system-mod/system-logs-item-dto.model';
/**
 * 系统日志
 */
@Injectable({ providedIn: 'root' })
export class SystemLogsService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SystemLogsFilterDto
   */
  filter(data: SystemLogsFilterDto): Observable<PageList<SystemLogsItemDto>> {
    const _url = `/api/SystemLogs/filter`;
    return this.request<PageList<SystemLogsItemDto>>('post', _url, data);
  }
}