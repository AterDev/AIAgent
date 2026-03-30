import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationFilterDto } from '../models/model-mod/application-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ApplicationItemDto } from '../models/model-mod/application-item-dto.model';
import { ApplicationAddDto } from '../models/model-mod/application-add-dto.model';
import { ApplicationUpdateDto } from '../models/model-mod/application-update-dto.model';
import { ApplicationDetailDto } from '../models/model-mod/application-detail-dto.model';
import { ApplicationCredentialResultDto } from '../models/model-mod/application-credential-result-dto.model';
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
  add(data: ApplicationAddDto): Observable<ApplicationCredentialResultDto> {
    const _url = `/api/Application`;
    return this.request<ApplicationCredentialResultDto>('post', _url, data);
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
   * Reset application secret
   * @param id
   */
  resetSecret(id: string): Observable<ApplicationCredentialResultDto> {
    const _url = `/api/Application/${id}/reset-secret`;
    return this.request<ApplicationCredentialResultDto>('post', _url);
  }
  /**
   * Delete 应用定义 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Application/${id}`;
    return this.request<boolean>('delete', _url);
  }
}