import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StorageProviderFilterDto } from '../models/system-mod/storage-provider-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { StorageProviderItemDto } from '../models/system-mod/storage-provider-item-dto.model';
import { StorageProviderAddDto } from '../models/system-mod/storage-provider-add-dto.model';
import { StorageProvider } from '../models/entity/storage-provider.model';
import { StorageProviderUpdateDto } from '../models/system-mod/storage-provider-update-dto.model';
import { StorageProviderDetailDto } from '../models/system-mod/storage-provider-detail-dto.model';
/**
 * 存储服务商
 */
@Injectable({ providedIn: 'root' })
export class StorageProviderService extends BaseService {
  /**
   * list 存储服务商 with page ✍️
   * @param data StorageProviderFilterDto
   */
  list(data: StorageProviderFilterDto): Observable<PageList<StorageProviderItemDto>> {
    const _url = `/api/StorageProvider/filter`;
    return this.request<PageList<StorageProviderItemDto>>('post', _url, data);
  }
  /**
   * Add 存储服务商 ✍️
   * @param data StorageProviderAddDto
   */
  add(data: StorageProviderAddDto): Observable<StorageProvider> {
    const _url = `/api/StorageProvider`;
    return this.request<StorageProvider>('post', _url, data);
  }
  /**
   * Update 存储服务商 ✍️
   * @param id
   * @param data StorageProviderUpdateDto
   */
  update(id: string, data: StorageProviderUpdateDto): Observable<boolean> {
    const _url = `/api/StorageProvider/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 存储服务商 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<StorageProviderDetailDto> {
    const _url = `/api/StorageProvider/${id}`;
    return this.request<StorageProviderDetailDto>('get', _url);
  }
  /**
   * Delete 存储服务商 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/StorageProvider/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 设置指定的存储服务商为活跃状态 ✍️
   * @param id
   */
  setActive(id: string): Observable<boolean> {
    const _url = `/api/StorageProvider/${id}/activate`;
    return this.request<boolean>('put', _url);
  }
}