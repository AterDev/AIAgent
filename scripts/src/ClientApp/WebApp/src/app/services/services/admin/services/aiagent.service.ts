import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIAgentFilterDto } from '../models/aiagent-mod/aiagent-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIAgentItemDto } from '../models/aiagent-mod/aiagent-item-dto.model';
import { AIAgentAddDto } from '../models/aiagent-mod/aiagent-add-dto.model';
import { AIAgent } from '../models/entity/aiagent.model';
import { AIAgentUpdateDto } from '../models/aiagent-mod/aiagent-update-dto.model';
import { AIAgentDetailDto } from '../models/aiagent-mod/aiagent-detail-dto.model';
/**
 * agent
 */
@Injectable({ providedIn: 'root' })
export class AIAgentService extends BaseService {
  /**
   * list agent with page ✍️
   * @param data AIAgentFilterDto
   */
  list(data: AIAgentFilterDto): Observable<PageList<AIAgentItemDto>> {
    const _url = `/api/AIAgent/filter`;
    return this.request<PageList<AIAgentItemDto>>('post', _url, data);
  }
  /**
   * Add agent ✍️
   * @param data AIAgentAddDto
   */
  add(data: AIAgentAddDto): Observable<AIAgent> {
    const _url = `/api/AIAgent`;
    return this.request<AIAgent>('post', _url, data);
  }
  /**
   * Update agent ✍️
   * @param id
   * @param data AIAgentUpdateDto
   */
  update(id: string, data: AIAgentUpdateDto): Observable<boolean> {
    const _url = `/api/AIAgent/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get agent Detail ✍️
   * @param id
   */
  detail(id: string): Observable<AIAgentDetailDto> {
    const _url = `/api/AIAgent/${id}`;
    return this.request<AIAgentDetailDto>('get', _url);
  }
  /**
   * Delete agent ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/AIAgent/${id}`;
    return this.request<boolean>('delete', _url);
  }
}