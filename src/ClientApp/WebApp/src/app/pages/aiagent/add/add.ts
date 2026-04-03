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
import { AIAgentAddDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-add-dto.model';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-aiagent-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class AIAgentAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  isLoading = signal(true);
  availableModels = signal<AIModelInfoItemDto[]>([]);
  applicationId?: string;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIAgentAdd>,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.buildForm();
    this.applicationId = data?.applicationId;
  }

  ngOnInit(): void {
    this.loadAvailableModels();
  }

  private loadAvailableModels(): void {
    const models$ = this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 100 });
    const permissions$ = this.applicationId
      ? this.adminClient.applicationModelPermission.list({ applicationId: this.applicationId, isEnabled: true, pageIndex: 1, pageSize: 200 })
      : of(null);

    forkJoin({ models: models$, permissions: permissions$ }).subscribe({
      next: ({ models, permissions }) => {
        const allModels = models.data || [];
        if (permissions) {
          const allowedIds = new Set((permissions.data || []).map((q: { aiModelInfoId: string }) => q.aiModelInfoId));
          this.availableModels.set(allModels.filter(q => !!q.id && allowedIds.has(q.id)));
        } else {
          this.availableModels.set(allModels);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100)]],
      description: [null, []],
      modelId: [null, [Validators.required]],
      systemPrompt: [null, []],
      tools: [[], []],
      enable: [true, []],
      isPublic: [false, []],
      applicationId: [this.applicationId ?? null, []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get modelId() { return this.form.get('modelId') as FormControl; }
  get systemPrompt() { return this.form.get('systemPrompt') as FormControl; }
  get tools() { return this.form.get('tools') as FormControl; }
  get enable() { return this.form.get('enable') as FormControl; }
  get isPublic() { return this.form.get('isPublic') as FormControl; }
  get applicationIdControl() { return this.form.get('applicationId') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue() as AIAgentAddDto;
    if (this.applicationId) {
      payload.isPublic = false;
      this.adminClient.applicationAgent.add(payload).subscribe(() => this.dialogRef.close(true));
      return;
    }

    this.adminClient.aIAgent.add(payload).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
