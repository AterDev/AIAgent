import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { AIAgentFilterDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-filter-dto.model';
import { AIAgentItemDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-item-dto.model';
import { AIAgentAdd } from '../add/add';
import { AIAgentEdit } from '../edit/edit';
import { AIAgentDetail } from '../detail/detail';

@Component({
  selector: 'app-aiagent-index',
  imports: [CommonListModules, CommonFormModules],
  templateUrl: './index.html',
  standalone: true
})
export class AIAgentIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: AIAgentFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<AIAgentItemDto>();
  displayedColumns = ["name", "modelId", "enable", "isTemplate", "actions"];

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
    this.adminClient.aIAgent.list(this.filterDto as AIAgentFilterDto).subscribe((res) => {
      this.dataSource.data = (res.data || []);
      this.total = (res.count ?? res.data?.length ?? this.dataSource.data.length);
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

  openAdd() {
    const ref = this.dialog.open(AIAgentAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(AIAgentEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(AIAgentDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.aIAgent.delete(id).subscribe(() => this.loadData()); }
    });
  }
}
