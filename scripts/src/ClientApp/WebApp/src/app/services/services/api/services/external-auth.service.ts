import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ExternalAuthService extends BaseService {
  /**
   * Microsft login
   * @param returnUrl
   */
  signInMicrosoft(returnUrl: string | null): Observable<any> {
    const _url = `/api/ExternalAuth/signin-microsoft?returnUrl=${returnUrl ?? ''}`;
    return this.request<any>('get', _url);
  }
  /**
   * Google login
   * @param returnUrl
   */
  signInGoogle(returnUrl: string | null): Observable<any> {
    const _url = `/api/ExternalAuth/signin-google?returnUrl=${returnUrl ?? ''}`;
    return this.request<any>('get', _url);
  }
  /**
   * GetToken
   * @param type
   * @param returnUrl
   */
  getToken(type: string | null, returnUrl: string | null): Observable<any> {
    const _url = `/api/ExternalAuth/getToken?type=${type ?? ''}&returnUrl=${returnUrl ?? ''}`;
    return this.request<any>('get', _url);
  }
}