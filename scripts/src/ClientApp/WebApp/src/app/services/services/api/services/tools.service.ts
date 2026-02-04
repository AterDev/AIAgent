import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { McpToolFilterDto } from '../models/mcp-mod/mcp-tool-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { McpToolItemDto } from '../models/mcp-mod/mcp-tool-item-dto.model';
import { ToolDefinitionDto } from '../models/mcp-mod/tool-definition-dto.model';
import { McpToolDetailDto } from '../models/mcp-mod/mcp-tool-detail-dto.model';
/**
 * Open platform tools
 */
@Injectable({ providedIn: 'root' })
export class ToolsService extends BaseService {
  /**
   * list
   * @param data McpToolFilterDto
   */
  list(data: McpToolFilterDto): Observable<PageList<McpToolItemDto>> {
    const _url = `/api/v1/tools/filter`;
    return this.request<PageList<McpToolItemDto>>('post', _url, data);
  }
  /**
   * definitions
   */
  definitions(): Observable<ToolDefinitionDto[]> {
    const _url = `/api/v1/tools/definitions`;
    return this.request<ToolDefinitionDto[]>('get', _url);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<McpToolDetailDto> {
    const _url = `/api/v1/tools/${id}`;
    return this.request<McpToolDetailDto>('get', _url);
  }
}