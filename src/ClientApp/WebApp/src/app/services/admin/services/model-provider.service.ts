import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModelProviderFilterDto } from '../models/model-mod/model-provider-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ModelProviderItemDto } from '../models/model-mod/model-provider-item-dto.model';
import { ModelProviderAddDto } from '../models/model-mod/model-provider-add-dto.model';
import { ModelProvider } from '../models/entity/model-provider.model';
import { ModelProviderUpdateDto } from '../models/model-mod/model-provider-update-dto.model';
import { ModelProviderDetailDto } from '../models/model-mod/model-provider-detail-dto.model';
/**
 * 模型提供商管理
 */
@Injectable({ providedIn: 'root' })
export class ModelProviderService extends BaseService {
  /**
   * list
   * @param data ModelProviderFilterDto
   */
  list(data: ModelProviderFilterDto): Observable<PageList<ModelProviderItemDto>> {
    const _url = `/api/ModelProvider/filter`;
    return this.request<PageList<ModelProviderItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ModelProviderAddDto
   */
  add(data: ModelProviderAddDto): Observable<ModelProvider> {
    const _url = `/api/ModelProvider`;
    return this.request<ModelProvider>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ModelProviderUpdateDto
   */
  update(id: string, data: ModelProviderUpdateDto): Observable<boolean> {
    const _url = `/api/ModelProvider/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ModelProviderDetailDto> {
    const _url = `/api/ModelProvider/${id}`;
    return this.request<ModelProviderDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ModelProvider/${id}`;
    return this.request<boolean>('delete', _url);
  }
}