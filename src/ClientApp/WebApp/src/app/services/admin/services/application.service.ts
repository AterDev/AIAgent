import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationFilterDto } from '../models/model-mod/application-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationItemDto } from '../models/model-mod/application-item-dto.model';
import { ApplicationAddDto } from '../models/model-mod/application-add-dto.model';
import { ApplicationDetailDto } from '../models/model-mod/application-detail-dto.model';
import { ApplicationUpdateDto } from '../models/model-mod/application-update-dto.model';
import { ApplicationApiKeyItemDto } from '../models/model-mod/application-api-key-item-dto.model';
import { ApplicationApiKeyAddDto } from '../models/model-mod/application-api-key-add-dto.model';
import { ApplicationApiKeyCredentialResultDto } from '../models/model-mod/application-api-key-credential-result-dto.model';
/**
 * 应用定义
 */
@Injectable({ providedIn: 'root' })
export class ApplicationService extends BaseService {
  /**
   * list 应用定义 with page ✍️
   * @param data ApplicationFilterDto
   */
  list(data: ApplicationFilterDto): Observable<PageList<ApplicationItemDto>> {
    const _url = `/api/Application/filter`;
    return this.request<PageList<ApplicationItemDto>>('post', _url, data);
  }
  /**
   * Add 应用定义 ✍️
   * @param data ApplicationAddDto
   */
  add(data: ApplicationAddDto): Observable<ApplicationDetailDto> {
    const _url = `/api/Application`;
    return this.request<ApplicationDetailDto>('post', _url, data);
  }
  /**
   * Update 应用定义 ✍️
   * @param id
   * @param data ApplicationUpdateDto
   */
  update(id: string, data: ApplicationUpdateDto): Observable<boolean> {
    const _url = `/api/Application/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 应用定义 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<ApplicationDetailDto> {
    const _url = `/api/Application/${id}`;
    return this.request<ApplicationDetailDto>('get', _url);
  }
  /**
   * Delete 应用定义 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Application/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 获取应用 ApiKey 列表
   * @param id string
   */
  listApiKeys(id: string): Observable<ApplicationApiKeyItemDto[]> {
    const _url = `/api/Application/${id}/api-keys`;
    return this.request<ApplicationApiKeyItemDto[]>('get', _url);
  }
  /**
   * 新增应用 ApiKey
   * @param id string
   * @param data ApplicationApiKeyAddDto
   */
  addApiKey(id: string, data: ApplicationApiKeyAddDto): Observable<ApplicationApiKeyCredentialResultDto> {
    const _url = `/api/Application/${id}/api-keys`;
    return this.request<ApplicationApiKeyCredentialResultDto>('post', _url, data);
  }
  /**
   * 删除应用 ApiKey
   * @param id string
   * @param apiKeyId string
   */
  deleteApiKey(id: string, apiKeyId: string): Observable<boolean> {
    const _url = `/api/Application/${id}/api-keys/${apiKeyId}`;
    return this.request<boolean>('delete', _url);
  }
}