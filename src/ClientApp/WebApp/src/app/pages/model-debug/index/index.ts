import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { JsonPipe } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

interface ModelDebugRequest {
  modelId: string;
  prompt: string;
  temperature?: number;
  maxTokens?: number;
  systemPrompt?: string;
}

interface ModelDebugResponse {
  content: string;
  model: string;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  finishReason: string;
  duration: number;
}

@Component({
  selector: 'app-model-debug-index',
  imports: [
    CommonFormModules,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    JsonPipe
  ],
  templateUrl: './index.html',
  styleUrls: ['./index.scss'],
  standalone: true
})
export class ModelDebugIndex implements OnInit, OnDestroy {
  i18nKeys = I18N_KEYS;

  private destroy$ = new Subject<void>();

  debugForm!: FormGroup;
  isLoading = signal(false);
  response = signal<ModelDebugResponse | null>(null);
  error = signal<string | null>(null);
  history = signal<Array<{ request: ModelDebugRequest; response: ModelDebugResponse; timestamp: Date }>>([]);

  availableModels = signal<Array<{ id: string; name: string; provider: string }>>([]);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadAvailableModels();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.debugForm = this.fb.group({
      modelId: ['', Validators.required],
      systemPrompt: ['You are a helpful AI assistant.'],
      prompt: ['', Validators.required],
      temperature: [0.7, [Validators.min(0), Validators.max(2)]],
      maxTokens: [1000, [Validators.min(1), Validators.max(32000)]]
    });
  }

  private loadAvailableModels(): void {
    this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const models = (res.data || []).map(m => ({
            id: m.id || '',
            name: m.name || '',
            provider: m.provider || ''
          }));
          this.availableModels.set(models);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.error.set(this.translate.instant('modelDebug.errors.loadModelsFailed') + ': ' + err.message);
        }
      });
  }

  onSubmit(): void {
    if (this.debugForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);
    this.response.set(null);

    const request: ModelDebugRequest = this.debugForm.value;
    const startTime = Date.now();

    // Mock API call - replace with actual API
    // TODO: Create actual debug endpoint in backend
    setTimeout(() => {
      try {
        const mockResponse: ModelDebugResponse = {
          content: `This is a mock response for testing. Model: ${request.modelId}, Prompt: ${request.prompt.substring(0, 50)}...`,
          model: request.modelId,
          promptTokens: Math.floor(Math.random() * 100) + 50,
          completionTokens: Math.floor(Math.random() * 200) + 100,
          totalTokens: 0,
          finishReason: 'stop',
          duration: Date.now() - startTime
        };
        mockResponse.totalTokens = mockResponse.promptTokens + mockResponse.completionTokens;

        this.response.set(mockResponse);
        this.isLoading.set(false);

        // Add to history
        const currentHistory = this.history();
        this.history.set([
          { request, response: mockResponse, timestamp: new Date() },
          ...currentHistory.slice(0, 9) // Keep last 10
        ]);
      } catch (err: any) {
        this.isLoading.set(false);
        this.error.set(this.translate.instant('modelDebug.errors.testFailed') + ': ' + err.message);
      }
    }, 1500);
  }

  clearHistory(): void {
    this.history.set([]);
  }

  loadFromHistory(item: { request: ModelDebugRequest }): void {
    this.debugForm.patchValue(item.request);
  }

  exportResponse(): void {
    const resp = this.response();
    if (!resp) return;

    const dataStr = JSON.stringify(resp, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `model-debug-${new Date().toISOString()}.json`;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
