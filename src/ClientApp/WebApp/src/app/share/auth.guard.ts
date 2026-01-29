import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private auth: AuthService,
  ) {
  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean | UrlTree {
    const url = state.url;

    // 登录页面允许匿名访问
    if (url.startsWith('/login') || url === '/') {
      return true;
    }

    // 刷新用户登录状态
    this.auth.updateUserLoginState();

    // 如果已登录，允许访问
    if (this.auth.isLogin) {
      return true;
    }

    // 未登录，重定向到登录页
    return this.router.parseUrl('/login');
  }

  canActivateChild(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean | UrlTree {
    return this.canActivate(next, state);
  }
}

