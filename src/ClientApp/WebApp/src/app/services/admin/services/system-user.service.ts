import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemUserFilterDto } from '../models/system-mod/system-user-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SystemUserItemDto } from '../models/system-mod/system-user-item-dto.model';
import { SystemUserAddDto } from '../models/system-mod/system-user-add-dto.model';
import { SystemUser } from '../models/entity/system-user.model';
import { SystemUserUpdateDto } from '../models/system-mod/system-user-update-dto.model';
import { SystemUserDetailDto } from '../models/system-mod/system-user-detail-dto.model';
import { LoginDto } from '../models/system-mod/login-dto.model';
import { AccessTokenDto } from '../models/share/access-token-dto.model';
import { ChangePasswordDto } from '../models/system-mod/change-password-dto.model';
import { UserInfoDto } from '../models/share/user-info-dto.model';
/**
 * 系统用户
 */
@Injectable({ providedIn: 'root' })
export class SystemUserService extends BaseService {
  /**
   * list 系统用户 with page ✍️
   * @param data SystemUserFilterDto
   */
  list(data: SystemUserFilterDto): Observable<PageList<SystemUserItemDto>> {
    const _url = `/api/SystemUser/filter`;
    return this.request<PageList<SystemUserItemDto>>('post', _url, data);
  }
  /**
   * Add 系统用户 ✍️
   * @param data SystemUserAddDto
   */
  add(data: SystemUserAddDto): Observable<SystemUser> {
    const _url = `/api/SystemUser`;
    return this.request<SystemUser>('post', _url, data);
  }
  /**
   * Update 系统用户 ✍️
   * @param id
   * @param data SystemUserUpdateDto
   */
  update(id: string, data: SystemUserUpdateDto): Observable<boolean> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * Get 系统用户 Detail ✍️
   * @param id
   */
  detail(id: string): Observable<SystemUserDetailDto> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<SystemUserDetailDto>('get', _url);
  }
  /**
   * Delete 系统用户 ✍️
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 用户登录
   * @param data LoginDto
   */
  login(data: LoginDto): Observable<AccessTokenDto> {
    const _url = `/api/SystemUser/login`;
    return this.request<AccessTokenDto>('post', _url, data);
  }
  /**
   * 修改密码
   * @param data ChangePasswordDto
   */
  changePassword(data: ChangePasswordDto): Observable<boolean> {
    const _url = `/api/SystemUser/change-password`;
    return this.request<boolean>('post', _url, data);
  }
  /**
   * 获取当前用户信息
   */
  getCurrentUserInfo(): Observable<UserInfoDto> {
    const _url = `/api/SystemUser/current`;
    return this.request<UserInfoDto>('get', _url);
  }
}