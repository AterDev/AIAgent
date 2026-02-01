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
import { RagDocumentIngestDto } from '../models/knowledge-base-mod/rag-document-ingest-dto.model';
import { DocumentParseRequestDto } from '../models/knowledge-base-mod/document-parse-request-dto.model';
import { DocumentParsingResultDto } from '../models/knowledge-base-mod/document-parsing-result-dto.model';
/**
 * 文档管理
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
  /**
   * 解析并向量化文档
   * @param id string
   * @param data RagDocumentIngestDto
   */
  ingest(id: string, data: RagDocumentIngestDto): Observable<boolean> {
    const _url = `/api/RagDocument/${id}/ingest`;
    return this.request<boolean>('post', _url, data);
  }
  /**
   * 解析文档
   * @param id string
   * @param data DocumentParseRequestDto
   */
  parseDocument(id: string, data: DocumentParseRequestDto): Observable<any> {
    const _url = `/api/RagDocument/${id}/parse`;
    return this.request<any>('post', _url, data);
  }
  /**
   * 获取解析结果
   * @param id string
   */
  getParsingResults(id: string): Observable<DocumentParsingResultDto[]> {
    const _url = `/api/RagDocument/${id}/parsing-results`;
    return this.request<DocumentParsingResultDto[]>('get', _url);
  }
  /**
   * 获取最新的解析结果
   * @param id string
   */
  getLatestParsingResult(id: string): Observable<DocumentParsingResultDto> {
    const _url = `/api/RagDocument/${id}/latest-parsing-result`;
    return this.request<DocumentParsingResultDto>('get', _url);
  }
}