import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ModelProviderAddDto } from 'src/app/services/admin/models/model-mod/model-provider-add-dto.model';
import { ModelProviderType } from 'src/app/services/admin/models/entity/model-provider-type.model';

@Component({
  selector: 'app-model-provider-add',
  imports: [CommonFormModules, MatCheckboxModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class ModelProviderAdd implements OnInit {

  i18nKeys = I18N_KEYS;
  ModelProviderType = ModelProviderType;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ModelProviderAdd>,
    private translate: TranslateService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
  }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100)]],
      baseUrl: [null, [Validators.required, Validators.maxLength(500)]],
      apiKey: [null, [Validators.required, Validators.maxLength(2000)]],
      providerType: [ModelProviderType.OpenAiCompatible, [Validators.required]],
      timeoutSeconds: [30, [Validators.required]],
      retryCount: [1, [Validators.required]],
      description: [null, [Validators.maxLength(500)]],
      isEnabled: [true, []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get baseUrl() { return this.form.get('baseUrl') as FormControl; }
  get apiKey() { return this.form.get('apiKey') as FormControl; }
  get providerType() { return this.form.get('providerType') as FormControl; }
  get timeoutSeconds() { return this.form.get('timeoutSeconds') as FormControl; }
  get retryCount() { return this.form.get('retryCount') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
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
    this.adminClient.modelProvider.add(this.form.value as ModelProviderAddDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
