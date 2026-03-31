import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { ApplicationApiKeyAddDto } from 'src/app/services/admin/models/model-mod/application-api-key-add-dto.model';
import { ApplicationApiKeyCredentialResultDto } from 'src/app/services/admin/models/model-mod/application-api-key-credential-result-dto.model';

export interface ApplicationApiKeyAddDialogData {
  applicationId: string;
  applicationName: string;
}

@Component({
  selector: 'app-application-api-key-add',
  standalone: true,
  imports: [CommonFormModules, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html'
})
export class ApplicationApiKeyAdd {
  i18nKeys = I18N_KEYS;

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ApplicationApiKeyAdd>,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationApiKeyAddDialogData
  ) {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100)]],
      apiKeyExpiresInMonths: [3, [Validators.required]],
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get apiKeyExpiresInMonths() { return this.form.get('apiKeyExpiresInMonths') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.adminClient.application.addApiKey(this.data.applicationId, this.form.value as ApplicationApiKeyAddDto)
      .subscribe((result: ApplicationApiKeyCredentialResultDto) => this.dialogRef.close(result));
  }

  close(): void {
    this.dialogRef.close();
  }
}