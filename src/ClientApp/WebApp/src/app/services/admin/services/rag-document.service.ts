import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RagDocumentFilterDto } from '../models/knowledge-base-mod/rag-document-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { RagDocumentItemDto } from '../models/knowledge-base-mod/rag-document-item-dto.model';
import { RagDocumentAddDto } from '../models/knowledge-base-mod/rag-document-add-dto.model';
import { RagDocument } from '../models/entity/rag-document.model';
import { RagDocumentUpdateDto } from '../models/knowledge-base-mod/rag-document-update-dto.model';
import { RagDocumentDetailDto } from '../models/knowledge-base-mod/rag-document-detail-dto.model';
/**
 * 文档管理（仅管理，不包含处理逻辑）
 */
@Injectable({ providedIn: 'root' })
export class RagDocumentService extends BaseService {
  /**
   * list
   * @param data RagDocumentFilterDto
   */
  list(data: RagDocumentFilterDto): Observable<PageList<RagDocumentItemDto>> {
    const _url = `/api/RagDocument/filter`;
    return this.request<PageList<RagDocumentItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data RagDocumentAddDto
   */
  add(data: RagDocumentAddDto): Observable<RagDocument> {
    const _url = `/api/RagDocument`;
    return this.request<RagDocument>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data RagDocumentUpdateDto
   */
  update(id: string, data: RagDocumentUpdateDto): Observable<boolean> {
    const _url = `/api/RagDocument/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<RagDocumentDetailDto> {
    const _url = `/api/RagDocument/${id}`;
    return this.request<RagDocumentDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/RagDocument/${id}`;
    return this.request<boolean>('delete', _url);
  }
}