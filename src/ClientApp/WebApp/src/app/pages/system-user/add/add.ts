import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemUserAddDto } from
  'src/app/services/admin/models/system-mod/system-user-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { GenderType } from 'src/app/services/admin/models/ater/gender-type.model';
import { forkJoin, Observable } from 'rxjs';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-mod/system-role-item-dto.model';
import { PageList } from 'src/app/services/admin/models/ater/page-list.model';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';



@Component({
  selector: 'app-add',
  imports: [...CommonFormModules],
  templateUrl: './add.html',
  styleUrls: ['./add.scss']
})
export class Add implements OnInit {
  i18nKeys = I18N_KEYS;
  GenderType = GenderType;
  formGroup!: FormGroup;
  data = {} as SystemUserAddDto;
  roles = [] as SystemRoleItemDto[];
  isLoading = signal(true);
  isProcessing = false;
  private adminClient = inject(AdminClient);

  constructor(
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<Add>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' },
    private translate: TranslateService
  ) {

  }

  get userName() { return this.formGroup.get('userName') as FormControl };
  get password() { return this.formGroup.get('password') as FormControl };
  get realName() { return this.formGroup.get('realName') as FormControl };
  get email() { return this.formGroup.get('email') as FormControl };
  get roleIds() { return this.formGroup.get('roleIds') as FormControl };

  ngOnInit(): void {

    this.initData();
  }

  initData(): void {

    forkJoin([this.getRoles()])
      .subscribe({
        next: ([roles]) => {
          if (roles.data) {
            this.roles = roles.data;
          }
        },
        complete: () => {
          this.isLoading.set(false);
          this.initForm();
        }
      });
  }
  getRoles(): Observable<PageList<SystemRoleItemDto>> {
    {
      return this.adminClient.systemRole.list(null, null, 1, 99, null);
    }
  }

  initForm(): void {
    this.formGroup = new FormGroup({
      userName: new FormControl(null, [Validators.required, Validators.maxLength(30)]),
      password: new FormControl(null, [Validators.required, Validators.maxLength(60)]),
      realName: new FormControl(null, [Validators.maxLength(30)]),
      email: new FormControl(null, [Validators.maxLength(100)]),
      roleIds: new FormControl(null, [Validators.required]),
    });
  }
  getValidatorMessage(controlName: string): string {
    const control = this.formGroup.get(controlName);
    if (!control || !control.errors) {
      return '';
    }
    const errors = control.errors;
    const errorKeys = Object.keys(errors);
    if (errorKeys.length === 0) {
      return '';
    }
    const key = errorKeys[0];
    const params = errors[key];
    const translationKey = `validation.${key.toLowerCase()}`;
    return this.translate.instant(translationKey, params);
  }

  add(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      const data = this.formGroup.value as SystemUserAddDto;
      this.adminClient.systemUser.add(data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open(this.translate.instant(this.i18nKeys.common.addSuccess));
              this.dialogRef.close(res);
              //this.router.navigate(['../index'], { relativeTo: this.route });
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isProcessing = false;
          },
          complete: () => {
            this.isProcessing = false;
          }
        });
    } else {
      this.snb.open(this.translate.instant(this.i18nKeys.common.formInvalid));
    }
  }

  back(): void {
    this.location.back();
  }
}

