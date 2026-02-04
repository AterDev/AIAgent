import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RagQueryRequest } from '../models/share/rag-query-request.model';
import { RagQueryResult } from '../models/share/rag-query-result.model';
/**
 * Open platform RAG search
 */
@Injectable({ providedIn: 'root' })
export class RagSearchService extends BaseService {
  /**
   * search
   * @param data RagQueryRequest
   */
  search(data: RagQueryRequest): Observable<RagQueryResult> {
    const _url = `/api/v1/rag/search`;
    return this.request<RagQueryResult>('post', _url, data);
  }
}