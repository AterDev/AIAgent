import { Injectable } from '@angular/core';
import { AccessTokenDto } from './admin/models/share/access-token-dto.model';
import { UserInfoDto } from './admin/models/share/user-info-dto.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  isLogin = false;
  isAdmin = false;
  userName?: string | null = null;
  id?: string | null = null;

  constructor() {
    this.updateUserLoginState();
  }

  /**
   * 保存访问令牌
   */
  saveToken(token: AccessTokenDto): void {
    this.isLogin = true;
    localStorage.setItem('accessToken', token.accessToken);
    if (token.refreshToken) {
      localStorage.setItem('refreshToken', token.refreshToken);
    }
  }

  /**
   * 保存用户信息
   */
  saveUserInfo(userinfo: UserInfoDto): void {
    this.isLogin = true;
    this.userName = userinfo.userName;
    this.id = userinfo.id.toString();
    localStorage.setItem('username', userinfo.userName);
    if (userinfo.id) {
      localStorage.setItem('userId', userinfo.id.toString());
    }
  }

  /**
   * 获取访问令牌
   */
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  /**
   * 刷新登录状态
   */
  updateUserLoginState(): void {
    const username = localStorage.getItem('username');
    const token = localStorage.getItem('accessToken');
    if (token && username) {
      this.userName = username;
      this.id = localStorage.getItem('userId');
      this.isLogin = true;
    } else {
      this.isLogin = false;
    }
  }

  /**
   * 登出
   */
  logout(): void {
    localStorage.clear();
    this.isLogin = false;
    this.userName = null;
    this.id = null;
  }
}
