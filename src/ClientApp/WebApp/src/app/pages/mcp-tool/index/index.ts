import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { McpToolFilterDto } from 'src/app/services/admin/models/mcp-mod/mcp-tool-filter-dto.model';
import { McpToolItemDto } from 'src/app/services/admin/models/mcp-mod/mcp-tool-item-dto.model';
import { McpToolAdd } from '../add/add';
import { McpToolEdit } from '../edit/edit';
import { McpToolDetail } from '../detail/detail';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';

@Component({
  selector: 'app-mcp-tool-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, EnumTextPipe],
  templateUrl: './index.html',
  standalone: true
})
export class McpToolIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: McpToolFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<McpToolItemDto>();
  displayedColumns = ["name", "toolType", "version", "isEnabled", "actions"];

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
    this.adminClient.mcpTool.list(this.filterDto as McpToolFilterDto).subscribe({
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

  openAdd() {
    const ref = this.dialog.open(McpToolAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(McpToolEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(McpToolDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.mcpTool.delete(id).subscribe(() => this.loadData()); }
    });
  }
}
