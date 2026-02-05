import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { WorkflowExecutionFilterDto } from 'src/app/services/admin/models/workflow-mod/workflow-execution-filter-dto.model';
import { WorkflowExecutionItemDto } from 'src/app/services/admin/models/workflow-mod/workflow-execution-item-dto.model';
import { WorkflowExecutionDetail } from '../detail/detail';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';

@Component({
  selector: 'app-workflow-execution-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, EnumTextPipe],
  templateUrl: './index.html',
  standalone: true
})
export class WorkflowExecutionIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: WorkflowExecutionFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<WorkflowExecutionItemDto>();
  displayedColumns = ["workflowId", "status", "durationMs", "completedTime", "actions"];

  isLoading = signal(true);

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
    this.adminClient.workflowExecution.list(this.filterDto as WorkflowExecutionFilterDto).subscribe({
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
    this.dialog.open(WorkflowExecutionDetail, { minWidth: '600px', data: { id } });
  }
}
