import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslateService } from '@ngx-translate/core';
import { TranslateModule } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ApplicationQuotaDetailDto } from 'src/app/services/admin/models/model-mod/application-quota-detail-dto.model';
import { ApplicationQuotaUpdateDto } from 'src/app/services/admin/models/model-mod/application-quota-update-dto.model';
import { QuotaPeriodType } from 'src/app/services/admin/models/entity/quota-period-type.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

interface ApplicationQuotaEditDialogData {
  applicationId: string;
  applicationName: string;
  id?: string;
}

@Component({
  selector: 'app-application-quota-edit',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardContent,
    MatCardActions,
  ],
  templateUrl: './edit.html',
  standalone: true
})
export class ApplicationQuotaEdit implements OnInit {
  i18nKeys = I18N_KEYS;
  quotaPeriodType = QuotaPeriodType;
  form: FormGroup;
  isLoading = signal(true);
  private fb = inject(FormBuilder);
  private adminClient = inject(AdminClient);
  private dialogRef = inject(MatDialogRef<ApplicationQuotaEdit>);
  private translate = inject(TranslateService);

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ApplicationQuotaEditDialogData,
  ) {
    this.form = this.fb.group({
      periodType: [QuotaPeriodType.Day, [Validators.required]],
      maxRequests: [null, [Validators.required, Validators.min(1)]],
      maxTokens: [null, [Validators.required, Validators.min(1)]],
      windowSeconds: [null, [Validators.required, Validators.min(1)]],
      isEnabled: [true, []],
    });
  }

  ngOnInit(): void {
    if (!this.data.id) {
      this.isLoading.set(false);
      return;
    }

    this.adminClient.applicationQuota.detail(this.data.id).subscribe({
      next: (res: ApplicationQuotaDetailDto) => {
        this.form.patchValue(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
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
    if (this.form.invalid || !this.data.id) return;

    const payload: ApplicationQuotaUpdateDto = {
      periodType: this.periodType.value,
      maxRequests: this.maxRequests.value,
      maxTokens: this.maxTokens.value,
      windowSeconds: this.windowSeconds.value,
      isEnabled: this.isEnabled.value,
    };

    this.adminClient.applicationQuota.update(this.data.id, payload).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) {
    this.dialogRef.close(result);
  }
}