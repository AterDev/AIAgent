import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModelInvocationFilterDto } from '../models/model-mod/model-invocation-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ModelInvocationItemDto } from '../models/model-mod/model-invocation-item-dto.model';
import { ModelInvocationAddDto } from '../models/model-mod/model-invocation-add-dto.model';
import { ModelInvocation } from '../models/entity/model-invocation.model';
import { ModelInvocationUpdateDto } from '../models/model-mod/model-invocation-update-dto.model';
import { ModelInvocationDetailDto } from '../models/model-mod/model-invocation-detail-dto.model';
/**
 * 模型调用记录管理
 */
@Injectable({ providedIn: 'root' })
export class ModelInvocationService extends BaseService {
  /**
   * list
   * @param data ModelInvocationFilterDto
   */
  list(data: ModelInvocationFilterDto): Observable<PageList<ModelInvocationItemDto>> {
    const _url = `/api/ModelInvocation/filter`;
    return this.request<PageList<ModelInvocationItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ModelInvocationAddDto
   */
  add(data: ModelInvocationAddDto): Observable<ModelInvocation> {
    const _url = `/api/ModelInvocation`;
    return this.request<ModelInvocation>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ModelInvocationUpdateDto
   */
  update(id: string, data: ModelInvocationUpdateDto): Observable<boolean> {
    const _url = `/api/ModelInvocation/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ModelInvocationDetailDto> {
    const _url = `/api/ModelInvocation/${id}`;
    return this.request<ModelInvocationDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ModelInvocation/${id}`;
    return this.request<boolean>('delete', _url);
  }
}