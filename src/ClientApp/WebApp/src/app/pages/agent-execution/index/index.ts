import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { AgentExecutionFilterDto } from 'src/app/services/admin/models/aiagent-mod/agent-execution-filter-dto.model';
import { AgentExecutionItemDto } from 'src/app/services/admin/models/aiagent-mod/agent-execution-item-dto.model';
import { AgentExecutionDetail } from '../detail/detail';

@Component({
  selector: 'app-agent-execution-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule],
  templateUrl: './index.html',
  standalone: true
})
export class AgentExecutionIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: AgentExecutionFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<AgentExecutionItemDto>();
  displayedColumns = ["agentId", "status", "durationMs", "completedTime", "actions"];

  isLoading = signal(false);

  total = 0;
  pageSize = 10;

  constructor(
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.agentExecution.list(this.filterDto as AgentExecutionFilterDto).subscribe({
      next: (res) => {
        this.dataSource.data = (res.data || []);
        this.total = (res.count ?? res.data?.length ?? this.dataSource.data.length);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  filter(): void {
    this.filterDto.pageIndex = 1;
    this.loadData();
  }

  pageChanged(e: any) {
    this.filterDto.pageIndex = e.pageIndex + 1;
    this.filterDto.pageSize = e.pageSize;
    this.loadData();
  }

  openDetail(id: string) {
    this.dialog.open(AgentExecutionDetail, { minWidth: '600px', data: { id } });
  }
}
