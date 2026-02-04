import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AIPromptFilterDto } from '../models/core-mod/aiprompt-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { AIPromptItemDto } from '../models/core-mod/aiprompt-item-dto.model';
import { AIPromptAddDto } from '../models/core-mod/aiprompt-add-dto.model';
import { AIPrompt } from '../models/entity/aiprompt.model';
import { AIPromptUpdateDto } from '../models/core-mod/aiprompt-update-dto.model';
import { AIPromptDetailDto } from '../models/core-mod/aiprompt-detail-dto.model';
/**
 * 提示词
 */
@Injectable({ providedIn: 'root' })
export class AIPromptService extends BaseService {
  /**
   * list 提示词 with page ✍️
   * @param data AIPromptFilterDto
   */
  list(data: AIPromptFilterDto): Observable<PageList<AIPromptItemDto>> {
    const _url = `/api/AIPrompt/filter`;
    return this.request<PageList<AIPromptItemDto>>('post', _url, data);
  }
  /**
   * Add 提示词 ✍️
   * @param data AIPromptAddDto
   */
  add(data: AIPromptAddDto): Observable<AIPrompt> {
    const _url = `/api/AIPrompt`;
    return this.request<AIPrompt>('post', _url, data);
  }
  /**
   * Update 提示词 ✍️
   * @param id
   * @param data AIPromptUpdateDto
   */
  update(id: string, data: AIPromptUpdateDto): Observable<boolean> {
    const _url = `/api/AIPrompt/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 提示词 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<AIPromptDetailDto> {
    const _url = `/api/AIPrompt/${id}`;
    return this.request<AIPromptDetailDto>('get', _url);
  }
  /**
   * Delete 提示词 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/AIPrompt/${id}`;
    return this.request<boolean>('delete', _url);
  }
}