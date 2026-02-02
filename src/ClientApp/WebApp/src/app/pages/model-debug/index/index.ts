import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AuthService } from 'src/app/services/auth.service';
import { marked } from 'marked';
import { DomSanitizer } from '@angular/platform-browser';
import { SecurityContext } from '@angular/core';
import { ModelDebugRequest } from 'src/app/services/admin/models/model-mod/model-debug-request.model';
import { ModelDebugResponse } from 'src/app/services/admin/models/model-mod/model-debug-response.model';
import { environment } from 'src/environments/environment';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-model-debug-index',
  imports: [
    CommonFormModules,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule
  ],
  templateUrl: './index.html',
  styleUrls: ['./index.scss'],
  standalone: true
})
export class ModelDebugIndex implements OnInit, OnDestroy {
  i18nKeys = I18N_KEYS;

  private destroy$ = new Subject<void>();
  private abortController: AbortController | null = null;
  private currentRequestId: string | null = null;

  debugForm!: FormGroup;
  isLoading = signal(true); // 仅用于初始页面加载
  isStreaming = signal(false); // 用于流式输出状态
  response = signal<ModelDebugResponse | null>(null);
  streamingResponse = signal<string>('');
  renderedHtml = signal<string>('');
  error = signal<string | null>(null);
  history = signal<Array<{ request: ModelDebugRequest; response: ModelDebugResponse; timestamp: Date }>>([]);

  availableModels = signal<Array<{ id: string; name: string; providerId: string }>>([]);
  availableApplications = signal<Array<{ id: string; name: string }>>([]);
  isAdmin = signal(false);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private translate: TranslateService,
    private authService: AuthService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.isAdmin.set(this.authService.isAdmin);
    this.updateApplicationValidators();
    this.loadAvailableModels();
    this.loadApplications();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.abortController?.abort();
  }

  private initForm(): void {
    this.debugForm = this.fb.group({
      applicationId: [''],
      modelId: ['', Validators.required],
      systemPrompt: ['You are a helpful AI assistant.'],
      prompt: ['', Validators.required],
      temperature: [0.7, [Validators.min(0), Validators.max(2)]],
      maxTokens: [1000, [Validators.min(1), Validators.max(32000)]]
    });
  }

  private updateApplicationValidators(): void {
    const applicationIdControl = this.applicationId;
    if (this.isAdmin()) {
      applicationIdControl.clearValidators();
    } else {
      applicationIdControl.setValidators([Validators.required]);
    }
    applicationIdControl.updateValueAndValidity();
  }

  get applicationId(): FormControl {
    return this.debugForm.get('applicationId') as FormControl;
  }

  get modelId(): FormControl {
    return this.debugForm.get('modelId') as FormControl;
  }

  get systemPrompt(): FormControl {
    return this.debugForm.get('systemPrompt') as FormControl;
  }

  get prompt(): FormControl {
    return this.debugForm.get('prompt') as FormControl;
  }

  get temperature(): FormControl {
    return this.debugForm.get('temperature') as FormControl;
  }

  get maxTokens(): FormControl {
    return this.debugForm.get('maxTokens') as FormControl;
  }

  private loadAvailableModels(): void {
    this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const models = (res.data || []).map(m => ({
            id: m.id || '',
            name: m.name || '',
            providerId: m.providerId || ''
          }));
          this.availableModels.set(models);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.error.set(this.translate.instant(this.i18nKeys.modelDebug.errors.loadModelsFailed) + ': ' + err.message);
        }
      });
  }

  private loadApplications(): void {
    this.adminClient.application.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const apps = (res.data || []).map(app => ({
            id: app.id || '',
            name: app.name || ''
          }));
          this.availableApplications.set(apps);
        },
        error: (err) => {
          this.error.set(this.translate.instant(this.i18nKeys.modelDebug.errors.loadApplicationsFailed) + ': ' + err.message);
        }
      });
  }

  onSubmit(): void {
    if (this.debugForm.invalid) {
      return;
    }

    this.isStreaming.set(true);
    this.error.set(null);
    this.response.set(null);
    this.streamingResponse.set('');
    this.renderedHtml.set('');

    const requestId = this.generateRequestId();
    const request: ModelDebugRequest = {
      applicationId: this.applicationId.value || null,
      modelId: this.modelId.value,
      systemPrompt: this.systemPrompt.value,
      prompt: this.prompt.value,
      temperature: this.temperature.value,
      maxTokens: this.maxTokens.value,
      requestId
    };

    this.currentRequestId = requestId;
    this.startStream(request);
  }

  stopRequest(): void {
    if (!this.currentRequestId) {
      return;
    }

    this.abortController?.abort();
    this.abortController = null;

    const token = this.authService.getAccessToken();
    const url = `${environment.admin_daemon}/api/ModelDebug/stop/${this.currentRequestId}`;
    fetch(url,
      {
        method: 'POST',
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      })
      .finally(() => {
        this.isStreaming.set(false);
      });
  }

  private async startStream(request: ModelDebugRequest): Promise<void> {
    this.abortController = new AbortController();
    const token = this.authService.getAccessToken();
    const url = `${environment.admin_daemon}/api/ModelDebug/stream`;

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        },
        body: JSON.stringify(request),
        signal: this.abortController.signal
      });

      if (!response.ok || !response.body) {
        throw new Error(await response.text());
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) {
          break;
        }
        buffer += decoder.decode(value, { stream: true });
        buffer = this.processSseBuffer(buffer, request);
      }
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        this.error.set(this.translate.instant(this.i18nKeys.modelDebug.errors.testFailed) + ': ' + err.message);
      }
      this.isStreaming.set(false);
    }
  }

  private processSseBuffer(buffer: string, request: ModelDebugRequest): string {
    const chunks = buffer.split('\n\n');
    for (let i = 0; i < chunks.length - 1; i++) {
      const chunk = chunks[i];
      const lines = chunk.split('\n');
      for (const line of lines) {
        if (!line.startsWith('data:')) {
          continue;
        }
        const payload = line.replace('data:', '').trim();
        if (!payload) {
          continue;
        }
        this.handleStreamEvent(payload, request);
      }
    }
    return chunks[chunks.length - 1];
  }

  private handleStreamEvent(payload: string, request: ModelDebugRequest): void {
    try {
      const evt = JSON.parse(payload);
      if (evt.type === 'delta' && evt.delta) {
        const content = this.streamingResponse() + evt.delta;
        this.streamingResponse.set(content);
        this.renderMarkdown(content);
      }
      if (evt.type === 'error') {
        this.error.set(evt.error || this.translate.instant(this.i18nKeys.modelDebug.errors.testFailed));
        this.isStreaming.set(false);
      }
      if (evt.type === 'final' && evt.final) {
        const finalResponse: ModelDebugResponse = {
          content: evt.final.content,
          model: evt.final.model,
          promptTokens: evt.final.promptTokens,
          completionTokens: evt.final.completionTokens,
          totalTokens: evt.final.totalTokens,
          finishReason: evt.final.finishReason,
          durationMs: evt.final.durationMs
        };
        this.response.set(finalResponse);
        this.streamingResponse.set(finalResponse.content);
        this.renderMarkdown(finalResponse.content);
        this.isStreaming.set(false);

        const currentHistory = this.history();
        this.history.set([
          { request, response: finalResponse, timestamp: new Date() },
          ...currentHistory.slice(0, 9)
        ]);
      }
    } catch (err: any) {
      this.error.set(this.translate.instant(this.i18nKeys.modelDebug.errors.testFailed) + ': ' + err.message);
      this.isStreaming.set(false);
    }
  }

  private generateRequestId(): string {
    return `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`;
  }

  private renderMarkdown(content: string): void {
    const html = marked.parse(content ?? '', { async: false }) as string;
    const sanitized = this.sanitizer.sanitize(SecurityContext.HTML, html) ?? '';
    this.renderedHtml.set(sanitized);
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
