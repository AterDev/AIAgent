import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIModelInfoFilterDto } from '../models/aiagent-mod/aimodel-info-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIModelInfoItemDto } from '../models/aiagent-mod/aimodel-info-item-dto.model';
import { AIModelInfoAddDto } from '../models/aiagent-mod/aimodel-info-add-dto.model';
import { AIModelInfo } from '../models/entity/aimodel-info.model';
import { AIModelInfoUpdateDto } from '../models/aiagent-mod/aimodel-info-update-dto.model';
import { AIModelInfoDetailDto } from '../models/aiagent-mod/aimodel-info-detail-dto.model';
/**
 * 模型信息
 */
@Injectable({ providedIn: 'root' })
export class AIModelInfoService extends BaseService {
  /**
   * list 模型信息 with page ✍️
   * @param data AIModelInfoFilterDto
   */
  list(data: AIModelInfoFilterDto): Observable<PageList<AIModelInfoItemDto>> {
    const _url = `/api/AIModelInfo/filter`;
    return this.request<PageList<AIModelInfoItemDto>>('post', _url, data);
  }
  /**
   * Add 模型信息 ✍️
   * @param data AIModelInfoAddDto
   */
  add(data: AIModelInfoAddDto): Observable<AIModelInfo> {
    const _url = `/api/AIModelInfo`;
    return this.request<AIModelInfo>('post', _url, data);
  }
  /**
   * Update 模型信息 ✍️
   * @param id
   * @param data AIModelInfoUpdateDto
   */
  update(id: string, data: AIModelInfoUpdateDto): Observable<boolean> {
    const _url = `/api/AIModelInfo/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 模型信息 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<AIModelInfoDetailDto> {
    const _url = `/api/AIModelInfo/${id}`;
    return this.request<AIModelInfoDetailDto>('get', _url);
  }
  /**
   * Delete 模型信息 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/AIModelInfo/${id}`;
    return this.request<boolean>('delete', _url);
  }
}