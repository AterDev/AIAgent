import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MCPServerInfoFilterDto } from '../models/aiagent-mod/mcpserver-info-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { MCPServerInfoItemDto } from '../models/aiagent-mod/mcpserver-info-item-dto.model';
import { MCPServerInfoAddDto } from '../models/aiagent-mod/mcpserver-info-add-dto.model';
import { MCPServerInfo } from '../models/entity/mcpserver-info.model';
import { MCPServerInfoUpdateDto } from '../models/aiagent-mod/mcpserver-info-update-dto.model';
import { MCPServerInfoDetailDto } from '../models/aiagent-mod/mcpserver-info-detail-dto.model';
/**
 * MCP Server 管理
 */
@Injectable({ providedIn: 'root' })
export class MCPServerInfoService extends BaseService {
  /**
   * list
   * @param data MCPServerInfoFilterDto
   */
  list(data: MCPServerInfoFilterDto): Observable<PageList<MCPServerInfoItemDto>> {
    const _url = `/api/MCPServerInfo/filter`;
    return this.request<PageList<MCPServerInfoItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data MCPServerInfoAddDto
   */
  add(data: MCPServerInfoAddDto): Observable<MCPServerInfo> {
    const _url = `/api/MCPServerInfo`;
    return this.request<MCPServerInfo>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data MCPServerInfoUpdateDto
   */
  update(id: string, data: MCPServerInfoUpdateDto): Observable<boolean> {
    const _url = `/api/MCPServerInfo/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<MCPServerInfoDetailDto> {
    const _url = `/api/MCPServerInfo/${id}`;
    return this.request<MCPServerInfoDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/MCPServerInfo/${id}`;
    return this.request<boolean>('delete', _url);
  }
}