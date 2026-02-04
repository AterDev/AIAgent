import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RagCollectionFilterDto } from '../models/knowledge-base-mod/rag-collection-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { RagCollectionItemDto } from '../models/knowledge-base-mod/rag-collection-item-dto.model';
import { RagCollectionAddDto } from '../models/knowledge-base-mod/rag-collection-add-dto.model';
import { RagCollection } from '../models/entity/rag-collection.model';
import { RagCollectionUpdateDto } from '../models/knowledge-base-mod/rag-collection-update-dto.model';
import { RagCollectionDetailDto } from '../models/knowledge-base-mod/rag-collection-detail-dto.model';
/**
 * 知识库管理
 */
@Injectable({ providedIn: 'root' })
export class RagCollectionService extends BaseService {
  /**
   * list
   * @param data RagCollectionFilterDto
   */
  list(data: RagCollectionFilterDto): Observable<PageList<RagCollectionItemDto>> {
    const _url = `/api/RagCollection/filter`;
    return this.request<PageList<RagCollectionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data RagCollectionAddDto
   */
  add(data: RagCollectionAddDto): Observable<RagCollection> {
    const _url = `/api/RagCollection`;
    return this.request<RagCollection>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data RagCollectionUpdateDto
   */
  update(id: string, data: RagCollectionUpdateDto): Observable<boolean> {
    const _url = `/api/RagCollection/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<RagCollectionDetailDto> {
    const _url = `/api/RagCollection/${id}`;
    return this.request<RagCollectionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/RagCollection/${id}`;
    return this.request<boolean>('delete', _url);
  }
}