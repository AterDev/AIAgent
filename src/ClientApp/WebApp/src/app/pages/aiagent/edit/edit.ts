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
import { AIAgentUpdateDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-update-dto.model';
import { AIAgentDetailDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-detail-dto.model';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-aiagent-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class AIAgentEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);
  availableModels = signal<AIModelInfoItemDto[]>([]);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIAgentEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    this.loadData();
  }

  private loadData(): void {
    const models$ = this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 100 });
    
    if (this.id) {
      const detail$ = this.adminClient.aIAgent.detail(this.id);
      forkJoin([models$, detail$]).subscribe({
        next: ([modelsRes, detailRes]) => {
          this.availableModels.set(modelsRes.data || []);
          this.form.patchValue(detailRes);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      models$.subscribe({
        next: (res) => {
          this.availableModels.set(res.data || []);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "name": [null, [Validators.required, Validators.maxLength(100)]],
      "description": [null, []],
      "modelId": [null, [Validators.required]],
      "systemPrompt": [null, []],
      "tools": [null, []],
      "enable": [null, []],
      "isTemplate": [null, []],
      "userId": [null, []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get modelId() { return this.form.get('modelId') as FormControl; }
  get systemPrompt() { return this.form.get('systemPrompt') as FormControl; }
  get tools() { return this.form.get('tools') as FormControl; }
  get enable() { return this.form.get('enable') as FormControl; }
  get isTemplate() { return this.form.get('isTemplate') as FormControl; }
  get userId() { return this.form.get('userId') as FormControl; }

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
    this.adminClient.aIAgent.update(this.id, this.form.value as AIAgentUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
