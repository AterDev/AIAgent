import { Component, inject, OnInit, AfterViewInit, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { AuthService } from 'src/app/services/auth.service';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { initStarfield } from './starfield';
import { form, FormField, required, FieldState, minLength, maxLength, ValidationError } from '@angular/forms/signals'
import { translateValidationError } from 'src/app/share/validation-helpers';

interface SystemLoginDto {
  userName: string;
  password: string;
}


@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule, FormField],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login implements OnInit, AfterViewInit {
  i18nKeys = I18N_KEYS;
  private adminClient = inject(AdminClient);
  private translate = inject(TranslateService);
  isLoading = signal(true);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    if (authService.isLogin) {
      this.router.navigate(['/application']);
    }
  }

  loginModel = signal<SystemLoginDto>({
    userName: '',
    password: ''
  })

  loginForm = form(this.loginModel, (schema) => {
    required(schema.userName);
    minLength(schema.userName, 4);
    maxLength(schema.userName, 60);
    required(schema.password);
    minLength(schema.password, 6);
    maxLength(schema.password, 60);
  });

  get userName() {
    return this.loginForm.userName;
  }
  get password() {
    return this.loginForm.password;
  }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    const canvas = document.getElementById('starfield') as HTMLCanvasElement | null;
    if (canvas) {
      initStarfield(canvas);
    }
  }

  getValidatorMessage(field: FieldState<string, string>): string {
    const errors = field.errors();
    if (!errors || errors.length === 0) {
      return '';
    }

    return translateValidationError(this.translate, errors[0]);
  }

  doLogin(): void {
    if (this.loginForm().invalid()) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.loginModel();
    const loginData = {
      userName: formValue.userName,
      password: formValue.password
    };

    this.adminClient.systemUser.login(loginData)
      .subscribe({
        next: (res) => {
          this.authService.saveToken(res);
          this.getUserInfo();
        },
        error: (error: any) => {
          this.isLoading.set(false);
          console.error('Login failed:', error);
          // 这里可以添加错误提示处理
        }
      });
  }

  getUserInfo(): void {
    this.adminClient.systemUser.getCurrentUserInfo()
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          this.authService.saveUserInfo(res);
          this.router.navigate(['/application']);
        },
        error: (error: any) => {
          this.isLoading.set(false);
          console.error('Get user info failed:', error);
        }
      });
  }

  onLoginBtnMove(event: MouseEvent): void {
    const target = event.currentTarget as HTMLElement | null;
    if (!target) {
      return;
    }

    const rect = target.getBoundingClientRect();
    target.style.setProperty('--mx', `${event.clientX - rect.left}px`);
    target.style.setProperty('--my', `${event.clientY - rect.top}px`);
  }

  onLoginBtnLeave(event: MouseEvent): void {
    const target = event.currentTarget as HTMLElement | null;
    if (!target) {
      return;
    }

    target.style.setProperty('--mx', '50%');
    target.style.setProperty('--my', '50%');
  }


  logout(): void {
    this.authService.logout();
  }
}
