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
import { AIModelProviderUpdateDto } from 'src/app/services/admin/models/model-mod/aimodel-provider-update-dto.model';
import { AIModelProviderDetailDto } from 'src/app/services/admin/models/model-mod/aimodel-provider-detail-dto.model';

@Component({
  selector: 'app-ai-model-provider-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class AIModelProviderEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIModelProviderEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.aIModelProvider.detail(this.id).subscribe({
        next: (res: AIModelProviderDetailDto) => {
          this.form.patchValue(res);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "name": [null, [Validators.required, Validators.maxLength(200)]],
      "description": [null, [Validators.maxLength(1000)]],
      "logoUrl": [null, [Validators.maxLength(500)]],
      "website": [null, [Validators.maxLength(500)]],
      "apiKey": [null, [Validators.maxLength(200)]],
      "baseUrl": [null, [Validators.maxLength(200)]]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get logoUrl() { return this.form.get('logoUrl') as FormControl; }
  get website() { return this.form.get('website') as FormControl; }
  get apiKey() { return this.form.get('apiKey') as FormControl; }
  get baseUrl() { return this.form.get('baseUrl') as FormControl; }

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
    this.adminClient.aIModelProvider.update(this.id, this.form.value as AIModelProviderUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
