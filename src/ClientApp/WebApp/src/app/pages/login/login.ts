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
import { form, Field, required, email, FieldState, minLength, maxLength, ValidationError } from '@angular/forms/signals'
import { SystemLoginDto } from 'src/app/services/admin/models/system-mod/system-login-dto.model';
import { translateValidationError } from 'src/app/share/validation-helpers';


@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule, Field],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login implements OnInit, AfterViewInit {
  i18nKeys = I18N_KEYS;
  private adminClient = inject(AdminClient);
  private translate = inject(TranslateService);
  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    if (authService.isLogin) {
      this.router.navigate(['/system-role']);
    }
  }

  loginModel = signal<SystemLoginDto>({
    email: '',
    password: ''
  })

  loginForm = form(this.loginModel, (schema) => {
    required(schema.email);
    email(schema.email);
    minLength(schema.email, 4);
    maxLength(schema.email, 100);
    required(schema.password);
    minLength(schema.password, 6);
    maxLength(schema.password, 60);
  });

  get email() {
    return this.loginForm.email;
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
    const data = this.loginForm().value();
    // 登录接口
    this.adminClient.systemUser.login(data)
      .subscribe(res => {
        this.authService.saveToken(res);
        this.getUserInfo();
      });
  }

  getUserInfo(): void {
    this.adminClient.systemUser.getUserInfo()
      .subscribe(res => {
        this.authService.saveUserInfo(res);
        this.router.navigate(['/system-role']);
      });
  }


  logout(): void {
    this.authService.logout();
  }
}
