import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemRoleUpdateDto } from
  'src/app/services/admin/models/system-mod/system-role-update-dto.model';
import { SystemRoleDetailDto } from
  'src/app/services/admin/models/system-mod/system-role-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-edit',
  imports: [...CommonFormModules],
  templateUrl: './edit.html',
  styleUrls: ['./edit.scss']
})
export class Edit implements OnInit {
  i18nKeys = I18N_KEYS;
  private adminClient = inject(AdminClient);

  formGroup!: FormGroup;
  id!: string;
  data = {} as SystemRoleDetailDto;
  updateData = {} as SystemRoleUpdateDto;
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

  get name() { return this.formGroup.get('name') as FormControl };
  get nameValue() { return this.formGroup.get('nameValue') as FormControl };
  get isSystem() { return this.formGroup.get('isSystem') as FormControl };


  ngOnInit(): void {
    this.getDetail();
  }

  getDetail(): void {
    this.adminClient.systemRole.detail(this.id)
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
      name: new FormControl(this.data.name, [Validators.maxLength(30)]),
      nameValue: new FormControl(this.data.nameValue, [Validators.maxLength(60)]),
      isSystem: new FormControl(this.data.isSystem, []),

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
      this.updateData = this.formGroup.value as SystemRoleUpdateDto;

      this.adminClient.systemRole.update(this.id, this.updateData)
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

