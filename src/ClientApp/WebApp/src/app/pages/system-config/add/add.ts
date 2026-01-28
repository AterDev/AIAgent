import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { SystemConfigAddDto } from 'src/app/services/admin/models/system-mod/system-config-add-dto.model';

@Component({
  selector: 'app-system-config-add',
  imports: [CommonFormModules, MatCheckboxModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class SystemConfigAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<SystemConfigAdd>,
    private translate: TranslateService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
  }

  buildForm() {
    this.form = this.fb.group({
      key: [null, [Validators.required, Validators.maxLength(100)]],
      value: [null, [Validators.maxLength(2000)]],
      description: [null, [Validators.maxLength(500)]],
      valid: [true, []],
      isSystem: [false, []],
      groupName: [null, [Validators.maxLength(60)]]
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
    this.adminClient.systemConfig.add(this.form.value as SystemConfigAddDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
