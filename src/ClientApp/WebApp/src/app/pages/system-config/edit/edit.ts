import { Component, Inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { SystemConfigUpdateDto } from 'src/app/services/admin/models/system-mod/system-config-update-dto.model';
import { SystemConfigDetailDto } from 'src/app/services/admin/models/system-mod/system-config-detail-dto.model';

@Component({
  selector: 'app-system-config-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class SystemConfigEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<SystemConfigEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.systemConfig.detail(this.id).subscribe({
        next: (res: SystemConfigDetailDto) => {
          this.form.patchValue(res);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "key": [null, [Validators.required, Validators.maxLength(100)]],
      "value": [null, [Validators.maxLength(2000)]],
      "description": [null, [Validators.maxLength(500)]],
      "valid": [null, []],
      "isSystem": [null, []],
      "groupName": [null, [Validators.maxLength(60)]]
    });
  }

  get key() { return this.form.get('key') as FormControl; }
  get value() { return this.form.get('value') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get valid() { return this.form.get('valid') as FormControl; }
  get isSystem() { return this.form.get('isSystem') as FormControl; }
  get groupName() { return this.form.get('groupName') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    if (!this.id) return;
    this.adminClient.systemConfig.update(this.id, this.form.value as SystemConfigUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
