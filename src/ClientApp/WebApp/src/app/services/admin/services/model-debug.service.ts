import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModelDebugRequest } from '../models/model-mod/model-debug-request.model';
import { ModelDebugResponse } from '../models/model-mod/model-debug-response.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ModelDebugService extends BaseService {
  /**
   * chat
   * @param data ModelDebugRequest
   */
  chat(data: ModelDebugRequest): Observable<ModelDebugResponse> {
    const _url = `/api/ModelDebug`;
    return this.request<ModelDebugResponse>('post', _url, data);
  }
  /**
   * stream
   * @param data ModelDebugRequest
   */
  stream(data: ModelDebugRequest): Observable<any> {
    const _url = `/api/ModelDebug/stream`;
    return this.request<any>('post', _url, data);
  }
  /**
   * stop
   * @param requestId string
   */
  stop(requestId: string): Observable<boolean> {
    const _url = `/api/ModelDebug/stop/${requestId}`;
    return this.request<boolean>('post', _url);
  }
}