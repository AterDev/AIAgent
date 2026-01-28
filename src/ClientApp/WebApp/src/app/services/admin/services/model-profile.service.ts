import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModelProfileFilterDto } from '../models/model-mod/model-profile-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ModelProfileItemDto } from '../models/model-mod/model-profile-item-dto.model';
import { ModelProfileAddDto } from '../models/model-mod/model-profile-add-dto.model';
import { ModelProfile } from '../models/entity/model-profile.model';
import { ModelProfileUpdateDto } from '../models/model-mod/model-profile-update-dto.model';
import { ModelProfileDetailDto } from '../models/model-mod/model-profile-detail-dto.model';
/**
 * 模型配置管理
 */
@Injectable({ providedIn: 'root' })
export class ModelProfileService extends BaseService {
  /**
   * list
   * @param data ModelProfileFilterDto
   */
  list(data: ModelProfileFilterDto): Observable<PageList<ModelProfileItemDto>> {
    const _url = `/api/ModelProfile/filter`;
    return this.request<PageList<ModelProfileItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ModelProfileAddDto
   */
  add(data: ModelProfileAddDto): Observable<ModelProfile> {
    const _url = `/api/ModelProfile`;
    return this.request<ModelProfile>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ModelProfileUpdateDto
   */
  update(id: string, data: ModelProfileUpdateDto): Observable<boolean> {
    const _url = `/api/ModelProfile/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ModelProfileDetailDto> {
    const _url = `/api/ModelProfile/${id}`;
    return this.request<ModelProfileDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ModelProfile/${id}`;
    return this.request<boolean>('delete', _url);
  }
}