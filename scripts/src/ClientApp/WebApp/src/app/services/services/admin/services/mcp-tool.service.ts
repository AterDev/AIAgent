import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { McpToolFilterDto } from '../models/mcp-mod/mcp-tool-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { McpToolItemDto } from '../models/mcp-mod/mcp-tool-item-dto.model';
import { ToolDefinitionDto } from '../models/mcp-mod/tool-definition-dto.model';
import { McpToolAddDto } from '../models/mcp-mod/mcp-tool-add-dto.model';
import { McpTool } from '../models/entity/mcp-tool.model';
import { McpToolUpdateDto } from '../models/mcp-mod/mcp-tool-update-dto.model';
import { McpToolDetailDto } from '../models/mcp-mod/mcp-tool-detail-dto.model';
/**
 * MCP 工具管理
 */
@Injectable({ providedIn: 'root' })
export class McpToolService extends BaseService {
  /**
   * list
   * @param data McpToolFilterDto
   */
  list(data: McpToolFilterDto): Observable<PageList<McpToolItemDto>> {
    const _url = `/api/McpTool/filter`;
    return this.request<PageList<McpToolItemDto>>('post', _url, data);
  }
  /**
   * definitions
   */
  definitions(): Observable<ToolDefinitionDto[]> {
    const _url = `/api/McpTool/definitions`;
    return this.request<ToolDefinitionDto[]>('get', _url);
  }
  /**
   * add
   * @param data McpToolAddDto
   */
  add(data: McpToolAddDto): Observable<McpTool> {
    const _url = `/api/McpTool`;
    return this.request<McpTool>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data McpToolUpdateDto
   */
  update(id: string, data: McpToolUpdateDto): Observable<boolean> {
    const _url = `/api/McpTool/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<McpToolDetailDto> {
    const _url = `/api/McpTool/${id}`;
    return this.request<McpToolDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/McpTool/${id}`;
    return this.request<boolean>('delete', _url);
  }
}