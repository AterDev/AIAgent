import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIAgentFilterDto } from '../models/aiagent-mod/aiagent-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIAgentItemDto } from '../models/aiagent-mod/aiagent-item-dto.model';
import { AIAgentDetailDto } from '../models/aiagent-mod/aiagent-detail-dto.model';
import { AgentExecuteRequestDto } from '../models/aiagent-mod/agent-execute-request-dto.model';
/**
 * Open platform agents
 */
@Injectable({ providedIn: 'root' })
export class AgentsService extends BaseService {
  /**
   * list
   * @param data AIAgentFilterDto
   */
  list(data: AIAgentFilterDto): Observable<PageList<AIAgentItemDto>> {
    const _url = `/api/v1/agents/filter`;
    return this.request<PageList<AIAgentItemDto>>('post', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<AIAgentDetailDto> {
    const _url = `/api/v1/agents/${id}`;
    return this.request<AIAgentDetailDto>('get', _url);
  }
  /**
   * Execute agent
   * @param id string
   * @param data AgentExecuteRequestDto
   */
  execute(id: string, data: AgentExecuteRequestDto): Observable<any> {
    const _url = `/api/v1/agents/${id}/execute`;
    return this.request<any>('post', _url, data);
  }
}