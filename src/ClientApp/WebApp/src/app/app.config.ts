import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { CustomerHttpInterceptor } from './customer-http.interceptor';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { environment } from 'src/environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideTranslateService({
      fallbackLang: 'en-US',
      lang: 'zh-CN',
      defaultLanguage: 'zh-CN',
      extend: true,
      useDefaultLang: true,
    }),
    provideTranslateHttpLoader({
      prefix: './assets/i18n/',
      suffix: '.json',
    }),
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
    { provide: 'ADMIN_BASE_URL', useValue: environment.admin_daemon, deps: [] },
  ],
};




