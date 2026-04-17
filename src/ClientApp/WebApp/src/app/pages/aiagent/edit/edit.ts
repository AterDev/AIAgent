import { Component, Inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { AIAgentUpdateDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-update-dto.model';
import { AIAgentDetailDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-detail-dto.model';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { AgentCapabilities } from 'src/app/services/admin/models/entity/agent-capabilities.model';
import { AgentMemoryMode } from 'src/app/services/admin/models/entity/agent-memory-mode.model';
import { forkJoin, of } from 'rxjs';
import {
  AGENT_CAPABILITY_OPTIONS,
  AGENT_MEMORY_OPTIONS,
  arrayToCapabilities,
  arrayToCsv,
  capabilitiesToArray,
  csvToArray,
} from '../maf-form-helpers';

@Component({
  selector: 'app-aiagent-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatExpansionModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class AIAgentEdit implements OnInit {

  i18nKeys = I18N_KEYS;
  capabilityOptions = AGENT_CAPABILITY_OPTIONS;
  memoryOptions = AGENT_MEMORY_OPTIONS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);
  availableModels = signal<AIModelInfoItemDto[]>([]);
  applicationId?: string;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIAgentEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
    this.applicationId = data?.applicationId;
  }

  ngOnInit() {
    this.loadData();
  }

  private loadData(): void {
    const models$ = this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 100 });
    const permissions$ = this.applicationId
      ? this.adminClient.applicationModelPermission.list({ applicationId: this.applicationId, isEnabled: true, pageIndex: 1, pageSize: 200 })
      : of(null);

    if (this.id) {
      const detail$ = this.applicationId
        ? this.adminClient.applicationAgent.detail(this.id)
        : this.adminClient.aIAgent.detail(this.id);
      forkJoin({ models: models$, permissions: permissions$, detail: detail$ }).subscribe({
        next: ({ models, permissions, detail }) => {
          this.applyModels(models.data || [], permissions);
          this.patchFromDetail(detail);
          if (this.applicationId) {
            this.isPublic.setValue(false);
          }
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      forkJoin({ models: models$, permissions: permissions$ }).subscribe({
        next: ({ models, permissions }) => {
          this.applyModels(models.data || [], permissions);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  private applyModels(allModels: AIModelInfoItemDto[], permissions: any): void {
    if (permissions) {
      const allowedIds = new Set((permissions.data || []).map((q: { aiModelInfoId: string }) => q.aiModelInfoId));
      this.availableModels.set(allModels.filter(q => !!q.id && allowedIds.has(q.id)));
    } else {
      this.availableModels.set(allModels);
    }
  }

  private patchFromDetail(detail: AIAgentDetailDto): void {
    this.form.patchValue({
      name: detail.name,
      description: detail.description,
      modelId: detail.modelId,
      systemPrompt: detail.systemPrompt,
      tools: detail.tools ?? [],
      handoffTargetsCsv: arrayToCsv(detail.handoffTargets),
      skillsCsv: arrayToCsv(detail.skills),
      tagsCsv: arrayToCsv(detail.tags),
      capabilityFlags: capabilitiesToArray(detail.capabilities),
      memoryMode: detail.memoryMode ?? AgentMemoryMode.None,
      contextWindow: detail.contextWindow ?? null,
      temperature: detail.temperature ?? null,
      topP: detail.topP ?? null,
      maxOutputTokens: detail.maxOutputTokens ?? null,
      responseSchemaJson: detail.responseSchemaJson ?? null,
      enable: detail.enable,
      isPublic: detail.isPublic,
      applicationId: detail.applicationId ?? this.applicationId ?? null,
    });
  }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100)]],
      description: [null, []],
      modelId: [null, [Validators.required]],
      systemPrompt: [null, []],
      tools: [[], []],
      handoffTargetsCsv: [''],
      skillsCsv: [''],
      tagsCsv: [''],
      capabilityFlags: [[] as AgentCapabilities[]],
      memoryMode: [AgentMemoryMode.None],
      contextWindow: [null],
      temperature: [null],
      topP: [null],
      maxOutputTokens: [null],
      responseSchemaJson: [null],
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
  get handoffTargetsCsv() { return this.form.get('handoffTargetsCsv') as FormControl; }
  get skillsCsv() { return this.form.get('skillsCsv') as FormControl; }
  get tagsCsv() { return this.form.get('tagsCsv') as FormControl; }
  get capabilityFlags() { return this.form.get('capabilityFlags') as FormControl; }
  get memoryMode() { return this.form.get('memoryMode') as FormControl; }
  get contextWindow() { return this.form.get('contextWindow') as FormControl; }
  get temperature() { return this.form.get('temperature') as FormControl; }
  get topP() { return this.form.get('topP') as FormControl; }
  get maxOutputTokens() { return this.form.get('maxOutputTokens') as FormControl; }
  get responseSchemaJson() { return this.form.get('responseSchemaJson') as FormControl; }
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
    if (!this.id) return;
    const raw = this.form.getRawValue();
    const payload: AIAgentUpdateDto = {
      name: raw.name,
      description: raw.description,
      modelId: raw.modelId,
      systemPrompt: raw.systemPrompt,
      tools: raw.tools ?? [],
      handoffTargets: csvToArray(raw.handoffTargetsCsv),
      skills: csvToArray(raw.skillsCsv),
      tags: csvToArray(raw.tagsCsv),
      capabilities: arrayToCapabilities(raw.capabilityFlags),
      memoryMode: raw.memoryMode ?? AgentMemoryMode.None,
      contextWindow: raw.contextWindow ?? null,
      temperature: raw.temperature ?? null,
      topP: raw.topP ?? null,
      maxOutputTokens: raw.maxOutputTokens ?? null,
      responseSchemaJson: raw.responseSchemaJson ?? null,
      providerId: null,
      enable: raw.enable,
      isPublic: raw.isPublic,
      applicationId: raw.applicationId,
    };
    if (this.applicationId) {
      payload.isPublic = false;
      this.adminClient.applicationAgent.update(this.id, payload).subscribe(() => this.dialogRef.close(true));
      return;
    }

    this.adminClient.aIAgent.update(this.id, payload).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
