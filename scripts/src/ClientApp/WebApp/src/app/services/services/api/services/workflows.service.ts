import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkflowFilterDto } from '../models/workflow-mod/workflow-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { WorkflowItemDto } from '../models/workflow-mod/workflow-item-dto.model';
import { WorkflowDetailDto } from '../models/workflow-mod/workflow-detail-dto.model';
import { WorkflowExecuteRequestDto } from '../models/api-service/workflow-execute-request-dto.model';
/**
 * Open platform workflows
 */
@Injectable({ providedIn: 'root' })
export class WorkflowsService extends BaseService {
  /**
   * list
   * @param data WorkflowFilterDto
   */
  list(data: WorkflowFilterDto): Observable<PageList<WorkflowItemDto>> {
    const _url = `/api/v1/workflows/filter`;
    return this.request<PageList<WorkflowItemDto>>('post', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<WorkflowDetailDto> {
    const _url = `/api/v1/workflows/${id}`;
    return this.request<WorkflowDetailDto>('get', _url);
  }
  /**
   * Execute workflow
   * @param id string
   * @param data WorkflowExecuteRequestDto
   */
  execute(id: string, data: WorkflowExecuteRequestDto): Observable<any> {
    const _url = `/api/v1/workflows/${id}/execute`;
    return this.request<any>('post', _url, data);
  }
}