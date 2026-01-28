import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkflowFilterDto } from '../models/workflow-mod/workflow-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { WorkflowItemDto } from '../models/workflow-mod/workflow-item-dto.model';
import { WorkflowAddDto } from '../models/workflow-mod/workflow-add-dto.model';
import { Workflow } from '../models/entity/workflow.model';
import { WorkflowUpdateDto } from '../models/workflow-mod/workflow-update-dto.model';
import { WorkflowDetailDto } from '../models/workflow-mod/workflow-detail-dto.model';
/**
 * 工作流管理
 */
@Injectable({ providedIn: 'root' })
export class WorkflowService extends BaseService {
  /**
   * list
   * @param data WorkflowFilterDto
   */
  list(data: WorkflowFilterDto): Observable<PageList<WorkflowItemDto>> {
    const _url = `/api/Workflow/filter`;
    return this.request<PageList<WorkflowItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data WorkflowAddDto
   */
  add(data: WorkflowAddDto): Observable<Workflow> {
    const _url = `/api/Workflow`;
    return this.request<Workflow>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data WorkflowUpdateDto
   */
  update(id: string, data: WorkflowUpdateDto): Observable<boolean> {
    const _url = `/api/Workflow/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<WorkflowDetailDto> {
    const _url = `/api/Workflow/${id}`;
    return this.request<WorkflowDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Workflow/${id}`;
    return this.request<boolean>('delete', _url);
  }
}