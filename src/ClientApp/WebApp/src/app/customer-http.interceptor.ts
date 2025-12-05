import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse, HttpResponse } from '@angular/common/http';

import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, throwError, from, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Injectable()
export class CustomerHttpInterceptor implements HttpInterceptor {
  constructor(
    private snb: MatSnackBar,
    private router: Router,
    private auth: AuthService
  ) {

  }
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          return this.handleError(error);
        })
      );
  }
  handleError(error: HttpErrorResponse) {
    if (error.error instanceof Blob) {
      return from(error.error.text()).pipe(
        switchMap((text: string) => {
          let errorBody = error.error;
          try {
            errorBody = JSON.parse(text);
          } catch (e) {
            console.error('Error parsing error blob', e);
          }
          const newError = new HttpErrorResponse({
            error: errorBody,
            headers: error.headers,
            status: error.status,
            url: error.url || undefined
          });
          return this.showError(newError);
        })
      );
    }
    return this.showError(error);
  }
  showError(error: HttpErrorResponse) {
    console.log(error);

    const errors = {
      detail: 'Server Error',
      status: 500,
    };

    switch (error.status) {
      case 401:
        errors.detail = '401: Unauthorized request';
        this.auth.logout();
        this.router.navigateByUrl('/login');
        break;
      case 403:
        errors.detail = '403: Forbidden request';
        break;
      case 404:
      case 409:
        errors.detail = error.error.detail;
        break;
      default:

        if (!error.error) {
          if (error.message) {
            errors.detail = error.message;
          }
          errors.status = error.status;
        } else {
          if (error.error.detail) {
            errors.detail = error.error.detail;
          }
          if (error.error.title) {
            errors.detail = error.error.title + ':' + errors.detail;
          }
        }
        break;
    }
    errors.status = error.status;
    this.snb.open(errors.detail, '了解', { duration: 10000 });
    return throwError(() => errors);
  }
}
