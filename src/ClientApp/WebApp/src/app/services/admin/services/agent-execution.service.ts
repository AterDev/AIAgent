import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AgentExecutionFilterDto } from '../models/aiagent-mod/agent-execution-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AgentExecutionItemDto } from '../models/aiagent-mod/agent-execution-item-dto.model';
import { AgentExecutionAddDto } from '../models/aiagent-mod/agent-execution-add-dto.model';
import { AgentExecution } from '../models/entity/agent-execution.model';
import { AgentExecutionUpdateDto } from '../models/aiagent-mod/agent-execution-update-dto.model';
import { AgentExecutionDetailDto } from '../models/aiagent-mod/agent-execution-detail-dto.model';
import { AgentExecuteRequestDto } from '../models/aiagent-mod/agent-execute-request-dto.model';
/**
 * Agent 执行管理
 */
@Injectable({ providedIn: 'root' })
export class AgentExecutionService extends BaseService {
  /**
   * list
   * @param data AgentExecutionFilterDto
   */
  list(data: AgentExecutionFilterDto): Observable<PageList<AgentExecutionItemDto>> {
    const _url = `/api/AgentExecution/filter`;
    return this.request<PageList<AgentExecutionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data AgentExecutionAddDto
   */
  add(data: AgentExecutionAddDto): Observable<AgentExecution> {
    const _url = `/api/AgentExecution`;
    return this.request<AgentExecution>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data AgentExecutionUpdateDto
   */
  update(id: string, data: AgentExecutionUpdateDto): Observable<boolean> {
    const _url = `/api/AgentExecution/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<AgentExecutionDetailDto> {
    const _url = `/api/AgentExecution/${id}`;
    return this.request<AgentExecutionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/AgentExecution/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 入队执行 Agent
   * @param id string
   * @param data AgentExecuteRequestDto
   */
  enqueue(id: string, data: AgentExecuteRequestDto): Observable<boolean> {
    const _url = `/api/AgentExecution/${id}/enqueue`;
    return this.request<boolean>('post', _url, data);
  }
}