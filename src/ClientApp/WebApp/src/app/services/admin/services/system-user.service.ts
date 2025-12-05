import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemLoginDto } from '../models/system-mod/system-login-dto.model';
import { AccessTokenDto } from '../models/share/access-token-dto.model';
import { UserInfoDto } from '../models/system-mod/user-info-dto.model';
import { PageList } from '../models/ater/page-list.model';
import { SystemUserItemDto } from '../models/system-mod/system-user-item-dto.model';
import { SystemUserAddDto } from '../models/system-mod/system-user-add-dto.model';
import { SystemUser } from '../models/entity/system-user.model';
import { SystemUserUpdateDto } from '../models/system-mod/system-user-update-dto.model';
import { SystemUserDetailDto } from '../models/system-mod/system-user-detail-dto.model';
/**
 * 系统用户
 */
@Injectable({ providedIn: 'root' })
export class SystemUserService extends BaseService {
  /**
   * 登录时，发送邮箱验证码 ✅
   * @param email
   */
  sendVerifyCode(email: string | null): Observable<any> {
    const _url = `/api/SystemUser/verifyCode?email=${email ?? ''}`;
    return this.request<any>('post', _url);
  }
  /**
   * 获取图形验证码 ✅
   */
  getCaptchaImage(): Observable<any> {
    const _url = `/api/SystemUser/captcha`;
    return this.request<any>('get', _url);
  }
  /**
   * 登录获取Token ✅
  返回仅包含 token 信息和用户 id/username
   * @param data SystemLoginDto
   */
  login(data: SystemLoginDto): Observable<AccessTokenDto> {
    const _url = `/api/SystemUser/authorize`;
    return this.request<AccessTokenDto>('post', _url, data);
  }
  /**
   * 根据用户 id 获取用户角色、菜单与权限组等信息
   */
  getUserInfo(): Observable<UserInfoDto> {
    const _url = `/api/SystemUser/userinfo`;
    return this.request<UserInfoDto>('get', _url);
  }
  /**
   * 刷新 token
   * @param refreshToken
   */
  refreshToken(refreshToken: string | null): Observable<AccessTokenDto> {
    const _url = `/api/SystemUser/refresh_token?refreshToken=${refreshToken ?? ''}`;
    return this.request<AccessTokenDto>('get', _url);
  }
  /**
   * 退出 ✅
   * @param id string
   */
  logout(id: string): Observable<boolean> {
    const _url = `/api/SystemUser/logout/${id}`;
    return this.request<boolean>('post', _url);
  }
  /**
   * 筛选 ✅
   * @param userName 用户名
   * @param roleId 角色id
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  filter(userName: string | null, roleId: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<SystemUserItemDto>> {
    const _url = `/api/SystemUser?userName=${userName ?? ''}&roleId=${roleId ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<SystemUserItemDto>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SystemUserAddDto
   */
  add(data: SystemUserAddDto): Observable<SystemUser> {
    const _url = `/api/SystemUser`;
    return this.request<SystemUser>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SystemUserUpdateDto
   */
  update(id: string, data: SystemUserUpdateDto): Observable<SystemUser> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<SystemUser>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SystemUserDetailDto> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<SystemUserDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<any> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * 修改密码 ✅
   * @param password string
   * @param newPassword string
   */
  changePassword(password: string | null, newPassword: string | null): Observable<boolean> {
    const _url = `/api/SystemUser/changePassword?password=${password ?? ''}&newPassword=${newPassword ?? ''}`;
    return this.request<boolean>('patch', _url);
  }
}