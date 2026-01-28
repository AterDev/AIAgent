import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ToolCallRecordFilterDto } from '../models/mcp-mod/tool-call-record-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ToolCallRecordItemDto } from '../models/mcp-mod/tool-call-record-item-dto.model';
import { ToolCallRecordAddDto } from '../models/mcp-mod/tool-call-record-add-dto.model';
import { ToolCallRecord } from '../models/entity/tool-call-record.model';
import { ToolCallRecordUpdateDto } from '../models/mcp-mod/tool-call-record-update-dto.model';
import { ToolCallRecordDetailDto } from '../models/mcp-mod/tool-call-record-detail-dto.model';
/**
 * MCP 调用记录管理
 */
@Injectable({ providedIn: 'root' })
export class ToolCallRecordService extends BaseService {
  /**
   * list
   * @param data ToolCallRecordFilterDto
   */
  list(data: ToolCallRecordFilterDto): Observable<PageList<ToolCallRecordItemDto>> {
    const _url = `/api/ToolCallRecord/filter`;
    return this.request<PageList<ToolCallRecordItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data ToolCallRecordAddDto
   */
  add(data: ToolCallRecordAddDto): Observable<ToolCallRecord> {
    const _url = `/api/ToolCallRecord`;
    return this.request<ToolCallRecord>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data ToolCallRecordUpdateDto
   */
  update(id: string, data: ToolCallRecordUpdateDto): Observable<boolean> {
    const _url = `/api/ToolCallRecord/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ToolCallRecordDetailDto> {
    const _url = `/api/ToolCallRecord/${id}`;
    return this.request<ToolCallRecordDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ToolCallRecord/${id}`;
    return this.request<boolean>('delete', _url);
  }
}