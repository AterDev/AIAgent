import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ChatMessageFilterDto } from '../models/aiagent-mod/chat-message-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { ChatMessageItemDto } from '../models/aiagent-mod/chat-message-item-dto.model';
import { ChatMessageAddDto } from '../models/aiagent-mod/chat-message-add-dto.model';
import { ChatMessage } from '../models/entity/chat-message.model';
import { ChatMessageUpdateDto } from '../models/aiagent-mod/chat-message-update-dto.model';
import { ChatMessageDetailDto } from '../models/aiagent-mod/chat-message-detail-dto.model';
/**
 * 聊天消息 控制器
 */
@Injectable({ providedIn: 'root' })
export class ChatMessageService extends BaseService {
  /**
   * 分页查询聊天消息
   * @param data ChatMessageFilterDto
   */
  list(data: ChatMessageFilterDto): Observable<PageList<ChatMessageItemDto>> {
    const _url = `/api/ChatMessage/filter`;
    return this.request<PageList<ChatMessageItemDto>>('post', _url, data);
  }
  /**
   * 新增聊天消息
   * @param data ChatMessageAddDto
   */
  add(data: ChatMessageAddDto): Observable<ChatMessage> {
    const _url = `/api/ChatMessage`;
    return this.request<ChatMessage>('post', _url, data);
  }
  /**
   * 更新聊天消息
   * @param id string
   * @param data ChatMessageUpdateDto
   */
  update(id: string, data: ChatMessageUpdateDto): Observable<boolean> {
    const _url = `/api/ChatMessage/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 获取聊天消息详情
   * @param id string
   */
  detail(id: string): Observable<ChatMessageDetailDto> {
    const _url = `/api/ChatMessage/${id}`;
    return this.request<ChatMessageDetailDto>('get', _url);
  }
  /**
   * 删除聊天消息
   * @param id string
   */
  delete(id: string): Observable<boolean | null> {
    const _url = `/api/ChatMessage/${id}`;
    return this.request<boolean | null>('delete', _url);
  }
}