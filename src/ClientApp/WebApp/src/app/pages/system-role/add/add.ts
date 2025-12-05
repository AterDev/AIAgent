import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemRoleAddDto } from
  'src/app/services/admin/models/system-mod/system-role-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/share/shared-modules';
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
  private adminClient = inject(AdminClient);

  formGroup!: FormGroup;
  data = {} as SystemRoleAddDto;
  isLoading = signal(true);
  isProcessing = false;
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

  get name() { return this.formGroup.get('name') as FormControl };
  get nameValue() { return this.formGroup.get('nameValue') as FormControl };
  get isSystem() { return this.formGroup.get('isSystem') as FormControl };

  ngOnInit(): void {
    this.initForm();
    this.isLoading.set(false);
  }

  initForm(): void {
    this.formGroup = new FormGroup({
      name: new FormControl(null, [Validators.required, Validators.maxLength(30)]),
      nameValue: new FormControl(null, [Validators.required, Validators.maxLength(60)]),
      isSystem: new FormControl(false, []),

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
      const data = this.formGroup.value as SystemRoleAddDto;
      this.adminClient.systemRole.add(data)
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

