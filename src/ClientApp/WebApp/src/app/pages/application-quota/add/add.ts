import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ApplicationQuotaAddDto } from 'src/app/services/admin/models/model-mod/application-quota-add-dto.model';
import { QuotaPeriodType } from 'src/app/services/admin/models/entity/quota-period-type.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';

export interface ApplicationQuotaFormDialogData {
  applicationId: string;
  applicationName: string;
  id?: string;
}

@Component({
  selector: 'app-application-quota-add',
  imports: [CommonFormModules, MatCheckboxModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './add.html',
  standalone: true
})
export class ApplicationQuotaAdd {
  i18nKeys = I18N_KEYS;
  quotaPeriodType = QuotaPeriodType;
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ApplicationQuotaAdd>,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationQuotaFormDialogData,
  ) {
    this.form = this.fb.group({
      periodType: [QuotaPeriodType.Day, [Validators.required]],
      maxRequests: [1000, [Validators.required, Validators.min(1)]],
      maxTokens: [2000000, [Validators.required, Validators.min(1)]],
      windowSeconds: [86400, [Validators.required, Validators.min(1)]],
      isEnabled: [true, []],
    });
  }

  get periodType() { return this.form.get('periodType') as FormControl; }
  get maxRequests() { return this.form.get('maxRequests') as FormControl; }
  get maxTokens() { return this.form.get('maxTokens') as FormControl; }
  get windowSeconds() { return this.form.get('windowSeconds') as FormControl; }
  get isEnabled() { return this.form.get('isEnabled') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;

    const payload: ApplicationQuotaAddDto = {
      applicationId: this.data.applicationId,
      periodType: this.periodType.value,
      maxRequests: this.maxRequests.value,
      maxTokens: this.maxTokens.value,
      windowSeconds: this.windowSeconds.value,
      isEnabled: this.isEnabled.value,
    };

    this.adminClient.applicationQuota.add(payload).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) {
    this.dialogRef.close(result);
  }
}