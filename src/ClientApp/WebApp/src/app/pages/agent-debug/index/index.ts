import { Component, OnInit, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
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

interface AgentDebugSession {
  id: string;
  agentId: string;
  agentName: string;
  messages: Array<{ role: string; content: string; timestamp: Date }>;
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
export class AgentDebugIndex implements OnInit {
  i18nKeys = I18N_KEYS;

  configForm!: FormGroup;
  testForm!: FormGroup;
  isLoading = signal(false);
  isTesting = signal(false);
  
  availableAgents = signal<Array<{ id: string; name: string; modelId: string }>>([]);
  availableTools = signal<Array<{ id: string; name: string }>>([]);
  
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
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadAgents();
    this.loadTools();
    this.loadExecutionHistory();
  }

  private initForms(): void {
    this.configForm = this.fb.group({
      agentId: ['', Validators.required],
      systemPrompt: [''],
      enabledTools: this.fb.array([]),
      temperature: [0.7],
      maxTokens: [2000],
      enableStreaming: [false],
      enableToolCallLogging: [true]
    });

    this.testForm = this.fb.group({
      userMessage: ['', Validators.required],
      contextMessages: this.fb.array([])
    });
  }

  private loadAgents(): void {
    this.isLoading.set(true);
    this.adminClient.aIAgent.list({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        const agents = (res.data || []).map(a => ({
          id: a.id || '',
          name: a.name || '',
          modelId: a.modelId || ''
        }));
        this.availableAgents.set(agents);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private loadTools(): void {
    this.adminClient.mcpTool.list({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        const tools = (res.data || []).map(t => ({
          id: t.id || '',
          name: t.name || ''
        }));
        this.availableTools.set(tools);
      }
    });
  }

  private loadExecutionHistory(): void {
    this.adminClient.agentExecution.list({ pageIndex: 1, pageSize: 20 }).subscribe({
      next: (res) => {
        const history = (res.data || []).map(e => this.mapExecutionToSession(e));
        this.executionHistory.set(history);
        this.dataSource.data = history;
      }
    });
  }

  private mapExecutionToSession(execution: any): AgentDebugSession {
    return {
      id: execution.id,
      agentId: execution.agentId,
      agentName: execution.agentName || 'Unknown',
      messages: JSON.parse(execution.messageHistory || '[]'),
      toolCalls: JSON.parse(execution.toolCalls || '[]'),
      status: execution.status === 'success' ? 'completed' : execution.status === 'failed' ? 'error' : 'running',
      error: execution.errorMessage,
      metrics: {
        duration: execution.durationMs || 0,
        tokenUsage: {
          prompt: execution.promptTokens || 0,
          completion: execution.completionTokens || 0,
          total: execution.totalTokens || 0
        },
        toolCallCount: execution.toolCallCount || 0
      }
    };
  }

  onAgentSelected(): void {
    const agent = this.selectedAgent();
    if (agent) {
      // Load agent details and populate form
      this.adminClient.aIAgent.detail(agent.id).subscribe({
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
    
    const startTime = Date.now();
    
    // Mock agent execution
    setTimeout(() => {
      const mockSession: AgentDebugSession = {
        id: Date.now().toString(),
        agentId: this.configForm.value.agentId,
        agentName: this.selectedAgent()?.name || '',
        messages: [
          { role: 'user', content: this.testForm.value.userMessage, timestamp: new Date() },
          { role: 'assistant', content: 'This is a mock response from the agent. In production, this would call the actual agent execution API.', timestamp: new Date() }
        ],
        toolCalls: [
          { name: 'search_knowledge', input: { query: 'test' }, output: { results: [] }, timestamp: new Date() }
        ],
        status: 'completed',
        metrics: {
          duration: Date.now() - startTime,
          tokenUsage: {
            prompt: 150,
            completion: 100,
            total: 250
          },
          toolCallCount: 1
        }
      };

      this.currentSession.set(mockSession);
      this.isTesting.set(false);

      // Add to history
      const history = this.executionHistory();
      this.executionHistory.set([mockSession, ...history]);
      this.dataSource.data = [mockSession, ...history];
    }, 2000);
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
}
