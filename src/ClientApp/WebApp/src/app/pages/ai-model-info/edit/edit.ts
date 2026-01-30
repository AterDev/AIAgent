import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AIModelInfoUpdateDto } from 'src/app/services/admin/models/model-mod/aimodel-info-update-dto.model';
import { AIModelInfoDetailDto } from 'src/app/services/admin/models/model-mod/aimodel-info-detail-dto.model';
import { AIModelProviderItemDto } from 'src/app/services/admin/models/model-mod/aimodel-provider-item-dto.model';
import { CommonFormModules } from 'src/app/share/shared-modules';

@Component({
  selector: 'app-ai-model-info-edit',
  imports: [CommonFormModules, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class AIModelInfoEdit implements OnInit {
  i18nKeys = I18N_KEYS;
  form!: FormGroup;
  isLoading = signal(true);
  id?: string;
  providers: AIModelProviderItemDto[] = [];

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIModelInfoEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit(): void {
    this.loadProviders();
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.aIModelInfo.detail(this.id).subscribe({
        next: (res: AIModelInfoDetailDto) => {
          this.form.patchValue(res);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }
  buildForm() {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      displayName: ['', Validators.maxLength(200)],
      description: ['', Validators.maxLength(1000)],
      providerId: ['', Validators.required],
      contextLength: [4096, [Validators.required, Validators.min(0)]],
      maxContextTokens: [8192, [Validators.required, Validators.min(0)]],
      supportsChat: [true],
      supportsEmbedding: [false],
      supportsTools: [false],
      supportsVision: [false],
      supportsResponsesApi: [false],
      inputPrice: [0, [Validators.required, Validators.min(0)]],
      outputPrice: [0, [Validators.required, Validators.min(0)]],
      isEnabled: [true]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get displayName() { return this.form.get('displayName') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get providerId() { return this.form.get('providerId') as FormControl; }
  get contextLength() { return this.form.get('contextLength') as FormControl; }
  get maxContextTokens() { return this.form.get('maxContextTokens') as FormControl; }
  get supportsChat() { return this.form.get('supportsChat') as FormControl; }
  get supportsEmbedding() { return this.form.get('supportsEmbedding') as FormControl; }
  get supportsTools() { return this.form.get('supportsTools') as FormControl; }
  get supportsVision() { return this.form.get('supportsVision') as FormControl; }
  get supportsResponsesApi() { return this.form.get('supportsResponsesApi') as FormControl; }
  get inputPrice() { return this.form.get('inputPrice') as FormControl; }
  get outputPrice() { return this.form.get('outputPrice') as FormControl; }
  get isEnabled() { return this.form.get('isEnabled') as FormControl; }
  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  loadProviders(): void {
    this.adminClient.aIModelProvider.list({ pageIndex: 1, pageSize: 1000 }).subscribe((res: any) => {
      this.providers = res.data || [];
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    if (!this.id) return;
    this.isLoading.set(true);
    this.adminClient.aIModelInfo.update(this.id, this.form.value as AIModelInfoUpdateDto).subscribe({
      next: () => {
        this.dialogRef.close(true);
      },
      error: () => this.isLoading.set(false)
    });
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
