import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RagAgentConfigFilterDto } from '../models/knowledge-base-mod/rag-agent-config-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { RagAgentConfigItemDto } from '../models/knowledge-base-mod/rag-agent-config-item-dto.model';
import { RagAgentConfigAddDto } from '../models/knowledge-base-mod/rag-agent-config-add-dto.model';
import { RagAgentConfig } from '../models/entity/rag-agent-config.model';
import { RagAgentConfigUpdateDto } from '../models/knowledge-base-mod/rag-agent-config-update-dto.model';
import { RagAgentConfigDetailDto } from '../models/knowledge-base-mod/rag-agent-config-detail-dto.model';
/**
 * RAG 模型配置
 */
@Injectable({ providedIn: 'root' })
export class RagAgentConfigService extends BaseService {
  /**
   * list RAG 模型配置 with page ✍️
   * @param data RagAgentConfigFilterDto
   */
  list(data: RagAgentConfigFilterDto): Observable<PageList<RagAgentConfigItemDto>> {
    const _url = `/api/RagAgentConfig/filter`;
    return this.request<PageList<RagAgentConfigItemDto>>('post', _url, data);
  }
  /**
   * Add RAG 模型配置 ✍️
   * @param data RagAgentConfigAddDto
   */
  add(data: RagAgentConfigAddDto): Observable<RagAgentConfig> {
    const _url = `/api/RagAgentConfig`;
    return this.request<RagAgentConfig>('post', _url, data);
  }
  /**
   * Update RAG 模型配置 ✍️
   * @param id
   * @param data RagAgentConfigUpdateDto
   */
  update(id: string, data: RagAgentConfigUpdateDto): Observable<boolean> {
    const _url = `/api/RagAgentConfig/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get RAG 模型配置 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<RagAgentConfigDetailDto> {
    const _url = `/api/RagAgentConfig/${id}`;
    return this.request<RagAgentConfigDetailDto>('get', _url);
  }
  /**
   * Delete RAG 模型配置 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/RagAgentConfig/${id}`;
    return this.request<boolean>('delete', _url);
  }
}