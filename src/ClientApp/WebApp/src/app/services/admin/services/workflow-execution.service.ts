import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkflowExecutionFilterDto } from '../models/workflow-mod/workflow-execution-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { WorkflowExecutionItemDto } from '../models/workflow-mod/workflow-execution-item-dto.model';
import { WorkflowExecutionAddDto } from '../models/workflow-mod/workflow-execution-add-dto.model';
import { WorkflowExecution } from '../models/entity/workflow-execution.model';
import { WorkflowExecutionUpdateDto } from '../models/workflow-mod/workflow-execution-update-dto.model';
import { WorkflowExecutionDetailDto } from '../models/workflow-mod/workflow-execution-detail-dto.model';
import { WorkflowExecutionProgress } from '../models/workflow-mod/workflow-execution-progress.model';
/**
 * 工作流执行管理
 */
@Injectable({ providedIn: 'root' })
export class WorkflowExecutionService extends BaseService {
  /**
   * list
   * @param data WorkflowExecutionFilterDto
   */
  list(data: WorkflowExecutionFilterDto): Observable<PageList<WorkflowExecutionItemDto>> {
    const _url = `/api/WorkflowExecution/filter`;
    return this.request<PageList<WorkflowExecutionItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data WorkflowExecutionAddDto
   */
  add(data: WorkflowExecutionAddDto): Observable<WorkflowExecution> {
    const _url = `/api/WorkflowExecution`;
    return this.request<WorkflowExecution>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data WorkflowExecutionUpdateDto
   */
  update(id: string, data: WorkflowExecutionUpdateDto): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<WorkflowExecutionDetailDto> {
    const _url = `/api/WorkflowExecution/${id}`;
    return this.request<WorkflowExecutionDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 入队执行工作流
   * @param id string
   */
  enqueue(id: string): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}/enqueue`;
    return this.request<boolean>('post', _url);
  }
  /**
   * 获取执行进度
   * @param id string
   */
  getProgress(id: string): Observable<WorkflowExecutionProgress> {
    const _url = `/api/WorkflowExecution/${id}/progress`;
    return this.request<WorkflowExecutionProgress>('get', _url);
  }
  /**
   * 断点续传执行
   * @param id string
   * @param fromStep number
   */
  resume(id: string, fromStep: number | null): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}/resume?fromStep=${fromStep ?? ''}`;
    return this.request<boolean>('post', _url);
  }
  /**
   * 重试失败的执行
   * @param id string
   */
  retry(id: string): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}/retry`;
    return this.request<boolean>('post', _url);
  }
  /**
   * 取消执行
   * @param id string
   */
  cancel(id: string): Observable<boolean> {
    const _url = `/api/WorkflowExecution/${id}/cancel`;
    return this.request<boolean>('post', _url);
  }
}