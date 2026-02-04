import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIModelProviderFilterDto } from '../models/model-mod/aimodel-provider-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIModelProviderItemDto } from '../models/model-mod/aimodel-provider-item-dto.model';
import { AIModelProviderAddDto } from '../models/model-mod/aimodel-provider-add-dto.model';
import { AIModelProvider } from '../models/entity/aimodel-provider.model';
import { AIModelProviderUpdateDto } from '../models/model-mod/aimodel-provider-update-dto.model';
import { AIModelProviderDetailDto } from '../models/model-mod/aimodel-provider-detail-dto.model';
/**
 * AI模型提供商
 */
@Injectable({ providedIn: 'root' })
export class AIModelProviderService extends BaseService {
  /**
   * list AI模型提供商 with page
   * @param data AIModelProviderFilterDto
   */
  list(data: AIModelProviderFilterDto): Observable<PageList<AIModelProviderItemDto>> {
    const _url = `/api/AIModelProvider/filter`;
    return this.request<PageList<AIModelProviderItemDto>>('post', _url, data);
  }
  /**
   * Add AI模型提供商
   * @param data AIModelProviderAddDto
   */
  add(data: AIModelProviderAddDto): Observable<AIModelProvider> {
    const _url = `/api/AIModelProvider`;
    return this.request<AIModelProvider>('post', _url, data);
  }
  /**
   * Update AI模型提供商
   * @param id string
   * @param data AIModelProviderUpdateDto
   */
  update(id: string, data: AIModelProviderUpdateDto): Observable<boolean> {
    const _url = `/api/AIModelProvider/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get AI模型提供商 Detail
   * @param id string
   */
  detail(id: string): Observable<AIModelProviderDetailDto> {
    const _url = `/api/AIModelProvider/${id}`;
    return this.request<AIModelProviderDetailDto>('get', _url);
  }
  /**
   * Delete AI模型提供商
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/AIModelProvider/${id}`;
    return this.request<boolean>('delete', _url);
  }
}