import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableDataSource } from '@angular/material/table';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { JsonPipe, DatePipe } from '@angular/common';
import { AuthService } from 'src/app/services/auth.service';
import { marked } from 'marked';
import { DomSanitizer } from '@angular/platform-browser';
import { SecurityContext } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Subject, takeUntil } from 'rxjs';

interface AgentDebugSession {
  id: string;
  agentId: string;
  agentName: string;
  messages: Array<{ role: string; content: string; html: string; timestamp: Date }>;
  toolCalls: Array<{ name: string; input: any; output: any; timestamp: Date }>;
  status: 'running' | 'completed' | 'error';
  error?: string;
  metrics: {
    duration: number;
    tokenUsage: { prompt: number; completion: number; total: number };
    toolCallCount: number;
  };
}

@Component({
  selector: 'app-agent-debug-index',
  imports: [
    CommonFormModules,
    CommonListModules,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    MatExpansionModule,
    JsonPipe,
    DatePipe
  ],
  templateUrl: './index.html',
  styleUrls: ['./index.scss'],
  standalone: true
})
export class AgentDebugIndex implements OnInit, OnDestroy {
  i18nKeys = I18N_KEYS;

  private destroy$ = new Subject<void>();
  private abortController: AbortController | null = null;
  private currentRequestId: string | null = null;

  configForm!: FormGroup;
  testForm!: FormGroup;
  isLoading = signal(false);
  isTesting = signal(false);
  isAdmin = signal(false);
  
  availableAgents = signal<Array<{ id: string; name: string }>>([]);
  availableTools = signal<Array<{ id: string; name: string }>>([]);
  availableApplications = signal<Array<{ id: string; name: string }>>([]);
  
  currentSession = signal<AgentDebugSession | null>(null);
  executionHistory = signal<AgentDebugSession[]>([]);
  
  displayedColumns = ['agentName', 'status', 'duration', 'tokens', 'toolCalls', 'timestamp', 'actions'];
  dataSource = new MatTableDataSource<AgentDebugSession>();

  selectedAgent = computed(() => {
    const agentId = this.configForm?.get('agentId')?.value;
    return this.availableAgents().find(a => a.id === agentId);
  });

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private translate: TranslateService,
    private authService: AuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.isAdmin.set(this.authService.isAdmin);
    this.updateApplicationValidators();
    this.loadAgents();
    this.loadTools();
    this.loadApplications();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.abortController?.abort();
  }

  private initForms(): void {
    this.configForm = this.fb.group({
      applicationId: [''],
      agentId: ['', Validators.required],
      systemPrompt: [''],
      enabledTools: [[]],
      temperature: [0.7],
      maxTokens: [2000],
      enableToolCallLogging: [true]
    });

    this.testForm = this.fb.group({
      userMessage: ['', Validators.required],
      contextMessages: this.fb.array([])
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
    return this.configForm.get('applicationId') as FormControl;
  }

  get agentId(): FormControl {
    return this.configForm.get('agentId') as FormControl;
  }

  get systemPrompt(): FormControl {
    return this.configForm.get('systemPrompt') as FormControl;
  }

  get enabledTools(): FormControl {
    return this.configForm.get('enabledTools') as FormControl;
  }

  get temperature(): FormControl {
    return this.configForm.get('temperature') as FormControl;
  }

  get maxTokens(): FormControl {
    return this.configForm.get('maxTokens') as FormControl;
  }

  get enableToolCallLogging(): FormControl {
    return this.configForm.get('enableToolCallLogging') as FormControl;
  }

  get userMessage(): FormControl {
    return this.testForm.get('userMessage') as FormControl;
  }

  private safeParseArray(jsonStr: string | null | undefined): any[] {
    if (!jsonStr) return [];
    try {
      const parsed = JSON.parse(jsonStr);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  private loadAgents(): void {
    this.isLoading.set(true);
    this.adminClient.aIAgent.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const agents = (res.data || []).map(a => ({
            id: a.id || '',
            name: a.name || ''
          }));
          this.availableAgents.set(agents);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }

  private loadTools(): void {
    this.adminClient.mcpTool.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const tools = (res.data || []).map(t => ({
            id: t.id || '',
            name: t.name || ''
          }));
          this.availableTools.set(tools);
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
          this.errorMessage(this.translate.instant('agentDebug.errors.loadApplicationsFailed') + ': ' + err.message);
        }
      });
  }

  onAgentSelected(): void {
    const agent = this.selectedAgent();
    if (agent) {
      // Load agent details and populate form
      this.adminClient.aIAgent.detail(agent.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (details) => {
            this.configForm.patchValue({
              systemPrompt: details.systemPrompt || '',
            });
          }
        });
    }
  }

  onTest(): void {
    if (this.configForm.invalid || this.testForm.invalid) {
      return;
    }

    this.isTesting.set(true);
    const requestId = this.generateRequestId();
    this.currentRequestId = requestId;

    const session: AgentDebugSession = {
      id: requestId,
      agentId: this.agentId.value,
      agentName: this.selectedAgent()?.name || '',
      messages: [],
      toolCalls: [],
      status: 'running',
      metrics: {
        duration: 0,
        tokenUsage: {
          prompt: 0,
          completion: 0,
          total: 0
        },
        toolCallCount: 0
      }
    };
    this.currentSession.set(session);

    const request = {
      applicationId: this.applicationId.value || null,
      agentId: this.agentId.value,
      systemPrompt: this.systemPrompt.value,
      userMessage: this.userMessage.value,
      temperature: this.temperature.value,
      maxTokens: this.maxTokens.value,
      enabledTools: this.enabledTools.value || [],
      enableToolCallLogging: this.configForm.get('enableToolCallLogging')?.value ?? true,
      requestId
    };

    this.startStream(request);
  }

  stopRequest(): void {
    if (!this.currentRequestId) {
      return;
    }

    this.abortController?.abort();
    this.abortController = null;

    const token = this.authService.getAccessToken();
    const url = `${environment.admin_daemon}/api/AgentDebug/stop/${this.currentRequestId}`;
    fetch(url,
      {
        method: 'POST',
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      })
      .finally(() => {
        this.isTesting.set(false);
      });
  }

  private async startStream(request: any): Promise<void> {
    this.abortController = new AbortController();
    const token = this.authService.getAccessToken();
    const url = `${environment.admin_daemon}/api/AgentDebug/stream`;

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
        buffer = this.processSseBuffer(buffer);
      }
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        this.errorMessage(this.translate.instant('agentDebug.errors.testFailed') + ': ' + err.message);
      }
      this.isTesting.set(false);
    }
  }

  private processSseBuffer(buffer: string): string {
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
        this.handleStreamEvent(payload);
      }
    }
    return chunks[chunks.length - 1];
  }

  private handleStreamEvent(payload: string): void {
    try {
      const evt = JSON.parse(payload);
      if (evt.type === 'message' && evt.message) {
        this.appendMessage(evt.message.role, evt.message.content, evt.message.timestamp);
      }
      if (evt.type === 'tool' && evt.toolCall) {
        this.appendToolCall(evt.toolCall);
      }
      if (evt.type === 'done' && evt.metrics) {
        this.completeSession(evt.metrics);
      }
      if (evt.type === 'error') {
        const errorMsg = evt.error || evt.message || this.translate.instant('agentDebug.errors.testFailed');
        this.failSession(errorMsg);
      }
    } catch (err: any) {
      this.failSession(this.translate.instant('agentDebug.errors.testFailed') + ': ' + err.message);
    }
  }

  private appendMessage(role: string, content: string, timestamp?: string): void {
    const session = this.currentSession();
    if (!session) return;

    const html = marked.parse(content ?? '', { async: false }) as string;
    const sanitized = this.sanitizer.sanitize(SecurityContext.HTML, html) ?? '';
    session.messages = [
      ...session.messages,
      {
        role,
        content,
        html: sanitized,
        timestamp: timestamp ? new Date(timestamp) : new Date()
      }
    ];

    this.currentSession.set({ ...session });
  }

  private appendToolCall(toolCall: any): void {
    const session = this.currentSession();
    if (!session) return;

    session.toolCalls = [
      ...session.toolCalls,
      {
        name: toolCall.name,
        input: toolCall.input,
        output: toolCall.output,
        timestamp: toolCall.timestamp ? new Date(toolCall.timestamp) : new Date()
      }
    ];

    this.currentSession.set({ ...session });
  }

  private completeSession(metrics: any): void {
    const session = this.currentSession();
    if (!session) return;

    session.status = 'completed';
    session.metrics.duration = metrics.durationMs || 0;
    session.metrics.tokenUsage = {
      prompt: metrics.promptTokens || 0,
      completion: metrics.completionTokens || 0,
      total: metrics.totalTokens || 0
    };
    session.metrics.toolCallCount = metrics.toolCallCount || 0;

    this.currentSession.set({ ...session });
    this.isTesting.set(false);

    const history = this.executionHistory();
    this.executionHistory.set([session, ...history]);
    this.dataSource.data = [session, ...history];
  }

  private failSession(message: string): void {
    const session = this.currentSession();
    if (session) {
      session.status = 'error';
      session.error = message;
      this.currentSession.set({ ...session });
    }
    this.isTesting.set(false);
  }

  private errorMessage(message: string): void {
    const session = this.currentSession();
    if (session) {
      session.error = message;
      session.status = 'error';
      this.currentSession.set({ ...session });
    }
  }

  viewSession(session: AgentDebugSession): void {
    this.currentSession.set(session);
  }

  clearSession(): void {
    this.currentSession.set(null);
    this.testForm.reset();
  }

  exportSession(): void {
    const session = this.currentSession();
    if (!session) return;

    const dataStr = JSON.stringify(session, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `agent-debug-${session.id}.json`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  rerunSession(session: AgentDebugSession): void {
    // Load session config and rerun
    const userMessage = session.messages.find(m => m.role === 'user')?.content || '';
    this.testForm.patchValue({ userMessage });
    this.onTest();
  }

  private generateRequestId(): string {
    return `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`;
  }
}
