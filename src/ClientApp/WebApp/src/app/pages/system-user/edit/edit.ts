import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemUserUpdateDto } from
  'src/app/services/admin/models/system-mod/system-user-update-dto.model';
import { SystemUserDetailDto } from
  'src/app/services/admin/models/system-mod/system-user-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { GenderType } from 'src/app/services/admin/models/ater/gender-type.model';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';


@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules, ToKeyValuePipe],
  templateUrl: './edit.html',
  styleUrls: ['./edit.scss']
})
export class Edit implements OnInit {
  i18nKeys = I18N_KEYS;
  GenderType = GenderType;
  private adminClient = inject(AdminClient);

  formGroup!: FormGroup;
  id!: string;
  data = {} as SystemUserDetailDto;
  updateData = {} as SystemUserUpdateDto;
  isLoading = signal(true);
  isProcessing = false;

  constructor(
    public snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<Edit>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' },
    private translate: TranslateService
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

  get userName() { return this.formGroup.get('userName') as FormControl };
  get password() { return this.formGroup.get('password') as FormControl };
  get realName() { return this.formGroup.get('realName') as FormControl };
  get email() { return this.formGroup.get('email') as FormControl };
  get phoneNumber() { return this.formGroup.get('phoneNumber') as FormControl };
  get avatar() { return this.formGroup.get('avatar') as FormControl };
  get sex() { return this.formGroup.get('sex') as FormControl };


  ngOnInit(): void {
    this.getDetail();
  }

  getDetail(): void {
    this.adminClient.systemUser.getDetail(this.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = res;
            this.initForm();
            this.isLoading.set(false);
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
            this.isLoading.set(false);
        }
      });
  }


  initForm(): void {
    this.formGroup = new FormGroup({
      userName: new FormControl(this.data.userName, [Validators.maxLength(30)]),
      password: new FormControl(null, [Validators.maxLength(60)]),
      realName: new FormControl(this.data.realName, [Validators.maxLength(30)]),
      email: new FormControl(this.data.email, [Validators.maxLength(100)]),
      phoneNumber: new FormControl(this.data.phoneNumber, [Validators.maxLength(20)]),
      avatar: new FormControl(this.data.avatar, [Validators.maxLength(200)]),
      sex: new FormControl(this.data.sex, []),

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

  edit(): void {
    if (this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as SystemUserUpdateDto;

      this.adminClient.systemUser.update(this.id, this.updateData)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open(this.translate.instant(this.i18nKeys.common.editSuccess));
              this.dialogRef.close(res);
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

