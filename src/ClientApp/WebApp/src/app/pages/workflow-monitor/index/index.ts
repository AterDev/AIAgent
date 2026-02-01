import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonListModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableDataSource } from '@angular/material/table';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { JsonPipe, DatePipe } from '@angular/common';
import { interval, Subject, takeUntil } from 'rxjs';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';

interface WorkflowStepExecution {
  stepId: string;
  stepName: string;
  stepType: string;
  status: 'pending' | 'running' | 'completed' | 'failed' | 'skipped';
  startTime?: Date;
  endTime?: Date;
  duration?: number;
  input?: any;
  output?: any;
  error?: string;
}

interface WorkflowExecutionDetail {
  id: string;
  workflowId: string;
  workflowName: string;
  status: 'running' | 'completed' | 'failed' | 'canceled';
  progress: number;
  steps: WorkflowStepExecution[];
  startTime: Date;
  endTime?: Date;
  duration?: number;
  inputParams?: any;
  result?: any;
  error?: string;
}

@Component({
  selector: 'app-workflow-monitor-index',
  imports: [
    CommonListModules,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    MatExpansionModule,
    MatProgressBarModule,
    JsonPipe,
    DatePipe
  ],
  templateUrl: './index.html',
  styleUrls: ['./index.scss'],
  standalone: true
})
export class WorkflowMonitorIndex implements OnInit, OnDestroy {
  i18nKeys = I18N_KEYS;

  private destroy$ = new Subject<void>();

  isLoading = signal(false);
  autoRefresh = signal(true);
  
  runningExecutions = signal<WorkflowExecutionDetail[]>([]);
  completedExecutions = signal<WorkflowExecutionDetail[]>([]);
  
  selectedExecution = signal<WorkflowExecutionDetail | null>(null);
  
  displayedColumns = ['workflowName', 'status', 'progress', 'startTime', 'duration', 'actions'];
  runningDataSource = new MatTableDataSource<WorkflowExecutionDetail>();
  completedDataSource = new MatTableDataSource<WorkflowExecutionDetail>();

  statusColors = {
    running: 'accent',
    completed: 'primary',
    failed: 'warn',
    canceled: ''
  };

  stepStatusIcons = {
    pending: 'schedule',
    running: 'autorenew',
    completed: 'check_circle',
    failed: 'error',
    skipped: 'skip_next'
  };

  stats = computed(() => {
    const running = this.runningExecutions();
    const completed = this.completedExecutions();
    
    return {
      totalRunning: running.length,
      totalCompleted: completed.length,
      successRate: completed.length > 0 
        ? ((completed.filter(e => e.status === 'completed').length / completed.length) * 100).toFixed(1)
        : '0',
      averageDuration: completed.length > 0
        ? Math.round(completed.reduce((sum, e) => sum + (e.duration || 0), 0) / completed.length)
        : 0
    };
  });

  constructor(
    private adminClient: AdminClient,
    private translate: TranslateService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadExecutions();
    
    // Auto-refresh every 5 seconds for running executions
    interval(5000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.autoRefresh()) {
          this.refreshRunningExecutions();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadExecutions(): void {
    this.isLoading.set(true);
    
    // Load running executions
    this.adminClient.workflowExecution.list({ pageIndex: 1, pageSize: 20, status: 'running' }).subscribe({
      next: (res) => {
        const executions = (res.data || []).map(e => this.mapToExecutionDetail(e));
        this.runningExecutions.set(executions);
        this.runningDataSource.data = executions;
      }
    });

    // Load completed executions
    this.adminClient.workflowExecution.list({ pageIndex: 1, pageSize: 50 }).subscribe({
      next: (res) => {
        const executions = (res.data || []).map(e => this.mapToExecutionDetail(e))
          .filter(e => e.status !== 'running');
        this.completedExecutions.set(executions);
        this.completedDataSource.data = executions;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private mapToExecutionDetail(execution: any): WorkflowExecutionDetail {
    const steps = this.parseStepsRecord(execution.stepsRecord);
    const completedSteps = steps.filter(s => s.status === 'completed').length;
    
    return {
      id: execution.id,
      workflowId: execution.workflowId,
      workflowName: execution.workflowName || 'Unknown Workflow',
      status: execution.status,
      progress: steps.length > 0 ? (completedSteps / steps.length) * 100 : 0,
      steps,
      startTime: new Date(execution.createdAt),
      endTime: execution.completedAt ? new Date(execution.completedAt) : undefined,
      duration: execution.durationMs,
      inputParams: execution.inputParameters ? JSON.parse(execution.inputParameters) : undefined,
      result: execution.result ? JSON.parse(execution.result) : undefined,
      error: execution.errorMessage
    };
  }

  private parseStepsRecord(stepsRecordJson?: string): WorkflowStepExecution[] {
    if (!stepsRecordJson) return [];
    
    try {
      const records = JSON.parse(stepsRecordJson);
      return records.map((r: any) => ({
        stepId: r.stepId,
        stepName: r.stepName || r.stepId,
        stepType: r.stepType,
        status: r.status,
        startTime: r.startTime ? new Date(r.startTime) : undefined,
        endTime: r.endTime ? new Date(r.endTime) : undefined,
        duration: r.duration,
        input: r.input,
        output: r.output,
        error: r.error
      }));
    } catch {
      return [];
    }
  }

  private refreshRunningExecutions(): void {
    this.adminClient.workflowExecution.list({ pageIndex: 1, pageSize: 20, status: 'running' }).subscribe({
      next: (res) => {
        const executions = (res.data || []).map(e => this.mapToExecutionDetail(e));
        this.runningExecutions.set(executions);
        this.runningDataSource.data = executions;

        // Update selected execution if it's running
        const selected = this.selectedExecution();
        if (selected && selected.status === 'running') {
          const updated = executions.find(e => e.id === selected.id);
          if (updated) {
            this.selectedExecution.set(updated);
          }
        }
      }
    });
  }

  viewExecution(execution: WorkflowExecutionDetail): void {
    // Load full details
    this.adminClient.workflowExecution.detail(execution.id).subscribe({
      next: (details) => {
        this.selectedExecution.set(this.mapToExecutionDetail(details));
      }
    });
  }

  cancelExecution(execution: WorkflowExecutionDetail): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('Are you sure you want to cancel this workflow execution?')
      }
    });
    
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        // TODO: Implement cancel endpoint
        console.log('Canceling execution:', execution.id);
      }
    });
  }

  retryExecution(execution: WorkflowExecutionDetail): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('Are you sure you want to retry this workflow?')
      }
    });
    
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        // TODO: Implement retry endpoint
        console.log('Retrying execution:', execution.id);
      }
    });
  }

  exportExecution(): void {
    const execution = this.selectedExecution();
    if (!execution) return;

    const dataStr = JSON.stringify(execution, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `workflow-execution-${execution.id}.json`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  toggleAutoRefresh(): void {
    this.autoRefresh.set(!this.autoRefresh());
  }

  refresh(): void {
    this.loadExecutions();
  }
}
