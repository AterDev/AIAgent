import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RagChunkFilterDto } from '../models/knowledge-base-mod/rag-chunk-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { RagChunkItemDto } from '../models/knowledge-base-mod/rag-chunk-item-dto.model';
import { RagChunkAddDto } from '../models/knowledge-base-mod/rag-chunk-add-dto.model';
import { RagChunk } from '../models/entity/rag-chunk.model';
import { RagChunkUpdateDto } from '../models/knowledge-base-mod/rag-chunk-update-dto.model';
import { RagChunkDetailDto } from '../models/knowledge-base-mod/rag-chunk-detail-dto.model';
/**
 * 文档分块管理
 */
@Injectable({ providedIn: 'root' })
export class RagChunkService extends BaseService {
  /**
   * list
   * @param data RagChunkFilterDto
   */
  list(data: RagChunkFilterDto): Observable<PageList<RagChunkItemDto>> {
    const _url = `/api/RagChunk/filter`;
    return this.request<PageList<RagChunkItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data RagChunkAddDto
   */
  add(data: RagChunkAddDto): Observable<RagChunk> {
    const _url = `/api/RagChunk`;
    return this.request<RagChunk>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data RagChunkUpdateDto
   */
  update(id: string, data: RagChunkUpdateDto): Observable<boolean> {
    const _url = `/api/RagChunk/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<RagChunkDetailDto> {
    const _url = `/api/RagChunk/${id}`;
    return this.request<RagChunkDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/RagChunk/${id}`;
    return this.request<boolean>('delete', _url);
  }
}