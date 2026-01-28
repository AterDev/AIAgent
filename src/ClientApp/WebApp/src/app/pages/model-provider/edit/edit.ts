import { Component, Inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ModelProviderUpdateDto } from 'src/app/services/admin/models/model-mod/model-provider-update-dto.model';
import { ModelProviderDetailDto } from 'src/app/services/admin/models/model-mod/model-provider-detail-dto.model';
import { ModelProviderType } from 'src/app/services/admin/models/entity/model-provider-type.model';

@Component({
  selector: 'app-model-provider-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class ModelProviderEdit implements OnInit {

  i18nKeys = I18N_KEYS;
  ModelProviderType = ModelProviderType;

  form!: FormGroup;
  id?: string;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ModelProviderEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.adminClient.modelProvider.detail(this.id).subscribe((res: ModelProviderDetailDto) => this.form.patchValue(res));
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "name": [null, [Validators.required, Validators.maxLength(100)]],
      "baseUrl": [null, [Validators.required, Validators.maxLength(500)]],
      "apiKey": [null, [Validators.required, Validators.maxLength(2000)]],
      "providerType": [null, [Validators.required]],
      "timeoutSeconds": [null, [Validators.required]],
      "retryCount": [null, [Validators.required]],
      "description": [null, [Validators.maxLength(500)]],
      "isEnabled": [null, []]
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
    if (!this.id) return;
    this.adminClient.modelProvider.update(this.id, this.form.value as ModelProviderUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
