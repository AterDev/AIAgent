import { Component, Inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ModelProfileUpdateDto } from 'src/app/services/admin/models/model-mod/model-profile-update-dto.model';
import { ModelProfileDetailDto } from 'src/app/services/admin/models/model-mod/model-profile-detail-dto.model';
import { ModelProviderItemDto } from 'src/app/services/admin/models/model-mod/model-provider-item-dto.model';

@Component({
  selector: 'app-model-profile-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class ModelProfileEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  providers: ModelProviderItemDto[] = [];

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ModelProfileEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    this.loadProviders();
    if (this.id) {
      this.adminClient.modelProfile.detail(this.id).subscribe((res: ModelProfileDetailDto) => this.form.patchValue(res));
    }
  }

  loadProviders(): void {
    this.adminClient.modelProvider.list({ pageIndex: 1, pageSize: 1000 }).subscribe((res) => {
      this.providers = res.data || [];
    });
  }

  buildForm() {
    this.form = this.fb.group({
      "providerId": [null, [Validators.required]],
      "name": [null, [Validators.required, Validators.maxLength(200)]],
      "displayName": [null, [Validators.maxLength(200)]],
      "description": [null, [Validators.maxLength(1000)]],
      "maxContextTokens": [null, [Validators.required]],
      "supportsChat": [null, []],
      "supportsEmbedding": [null, []],
      "supportsTools": [null, []],
      "supportsVision": [null, []],
      "supportsResponsesApi": [null, []],
      "isEnabled": [null, []]
    });
  }

  get providerId() { return this.form.get('providerId') as FormControl; }
  get name() { return this.form.get('name') as FormControl; }
  get displayName() { return this.form.get('displayName') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get maxContextTokens() { return this.form.get('maxContextTokens') as FormControl; }
  get supportsChat() { return this.form.get('supportsChat') as FormControl; }
  get supportsEmbedding() { return this.form.get('supportsEmbedding') as FormControl; }
  get supportsTools() { return this.form.get('supportsTools') as FormControl; }
  get supportsVision() { return this.form.get('supportsVision') as FormControl; }
  get supportsResponsesApi() { return this.form.get('supportsResponsesApi') as FormControl; }
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
    this.adminClient.modelProfile.update(this.id, this.form.value as ModelProfileUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
