import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConversationFilterDto } from '../models/aiagent-mod/conversation-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ConversationItemDto } from '../models/aiagent-mod/conversation-item-dto.model';
import { ConversationAddDto } from '../models/aiagent-mod/conversation-add-dto.model';
import { Conversation } from '../models/entity/conversation.model';
import { ConversationUpdateDto } from '../models/aiagent-mod/conversation-update-dto.model';
import { ConversationDetailDto } from '../models/aiagent-mod/conversation-detail-dto.model';
/**
 * 对话实例
 */
@Injectable({ providedIn: 'root' })
export class ConversationService extends BaseService {
  /**
   * 分页查询对话实例
   * @param data ConversationFilterDto
   */
  list(data: ConversationFilterDto): Observable<PageList<ConversationItemDto>> {
    const _url = `/api/Conversation/filter`;
    return this.request<PageList<ConversationItemDto>>('post', _url, data);
  }
  /**
   * 新增对话实例
   * @param data ConversationAddDto
   */
  add(data: ConversationAddDto): Observable<Conversation> {
    const _url = `/api/Conversation`;
    return this.request<Conversation>('post', _url, data);
  }
  /**
   * 更新对话实例
   * @param id string
   * @param data ConversationUpdateDto
   */
  update(id: string, data: ConversationUpdateDto): Observable<boolean> {
    const _url = `/api/Conversation/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 获取对话实例详情
   * @param id string
   */
  detail(id: string): Observable<ConversationDetailDto> {
    const _url = `/api/Conversation/${id}`;
    return this.request<ConversationDetailDto>('get', _url);
  }
  /**
   * 删除对话实例
   * @param id string
   */
  delete(id: string): Observable<boolean | null> {
    const _url = `/api/Conversation/${id}`;
    return this.request<boolean | null>('delete', _url);
  }
}