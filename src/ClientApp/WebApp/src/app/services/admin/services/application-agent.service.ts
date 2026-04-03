import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIAgentFilterDto } from '../models/aiagent-mod/aiagent-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIAgentItemDto } from '../models/aiagent-mod/aiagent-item-dto.model';
import { AIAgentAddDto } from '../models/aiagent-mod/aiagent-add-dto.model';
import { ApplicationAgent } from '../models/entity/application-agent.model';
import { AIAgentUpdateDto } from '../models/aiagent-mod/aiagent-update-dto.model';
import { AIAgentDetailDto } from '../models/aiagent-mod/aiagent-detail-dto.model';

/**
 * 应用侧 agent
 */
@Injectable({ providedIn: 'root' })
export class ApplicationAgentService extends BaseService {
  list(data: AIAgentFilterDto): Observable<PageList<AIAgentItemDto>> {
    const _url = `/api/ApplicationAgent/filter`;
    return this.request<PageList<AIAgentItemDto>>('post', _url, data);
  }

  add(data: AIAgentAddDto): Observable<ApplicationAgent> {
    const _url = `/api/ApplicationAgent`;
    return this.request<ApplicationAgent>('post', _url, data);
  }

  update(id: string, data: AIAgentUpdateDto): Observable<boolean> {
    const _url = `/api/ApplicationAgent/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  detail(id: string): Observable<AIAgentDetailDto> {
    const _url = `/api/ApplicationAgent/${id}`;
    return this.request<AIAgentDetailDto>('get', _url);
  }

  delete(id: string): Observable<boolean> {
    const _url = `/api/ApplicationAgent/${id}`;
    return this.request<boolean>('delete', _url);
  }
}