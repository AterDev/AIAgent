import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AgentDebugRequest } from '../models/aiagent-mod/agent-debug-request.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class AgentDebugService extends BaseService {
  /**
   * stream
   * @param data AgentDebugRequest
   */
  stream(data: AgentDebugRequest): Observable<any> {
    const _url = `/api/AgentDebug/stream`;
    return this.request<any>('post', _url, data);
  }
  /**
   * stop
   * @param requestId string
   */
  stop(requestId: string): Observable<boolean> {
    const _url = `/api/AgentDebug/stop/${requestId}`;
    return this.request<boolean>('post', _url);
  }
}