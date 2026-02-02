import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModelDebugRequestDto } from '../models/model-mod/model-debug-request-dto.model';
import { ModelDebugResponseDto } from '../models/model-mod/model-debug-response-dto.model';

/**
 * 模型调试工具
 */
@Injectable({ providedIn: 'root' })
export class ModelDebugService extends BaseService {
  /**
   * 调试模型调用
   * @param data ModelDebugRequestDto
   */
  debug(data: ModelDebugRequestDto): Observable<ModelDebugResponseDto> {
    const _url = `/api/ModelDebug`;
    return this.request<ModelDebugResponseDto>('post', _url, data);
  }
}
