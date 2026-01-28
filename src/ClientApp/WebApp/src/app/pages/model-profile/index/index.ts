import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ModelProfileFilterDto } from 'src/app/services/admin/models/model-mod/model-profile-filter-dto.model';
import { ModelProfileItemDto } from 'src/app/services/admin/models/model-mod/model-profile-item-dto.model';
import { ModelProviderItemDto } from 'src/app/services/admin/models/model-mod/model-provider-item-dto.model';
import { ModelProfileAdd } from '../add/add.js';
import { ModelProfileEdit } from '../edit/edit.js';
import { ModelProfileDetail } from '../detail/detail.js';

@Component({
  selector: 'app-model-profile-index',
  imports: [CommonListModules, CommonFormModules],
  templateUrl: './index.html',
  standalone: true
})
export class ModelProfileIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: ModelProfileFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<ModelProfileItemDto>();
  displayedColumns = [ "name", "displayName", "providerId", "isEnabled", "actions" ];

  total = 0;
  pageSize = 10;
  providers: ModelProviderItemDto[] = [];

  constructor(
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.loadProviders();
    this.loadData();
  }

  loadProviders(): void {
    this.adminClient.modelProvider.list({ pageIndex: 1, pageSize: 1000 }).subscribe((res) => {
      this.providers = res.data || [];
    });
  }

  loadData(): void {
    this.adminClient.modelProfile.list(this.filterDto as ModelProfileFilterDto).subscribe((res) => {
      this.dataSource.data = (res.data || []);
      this.total = (res.count ?? res.data?.length ?? this.dataSource.data.length);
    });
  }

  getProviderName(providerId: string): string {
    return this.providers.find(p => p.id === providerId)?.name || providerId;
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
    const ref = this.dialog.open(ModelProfileAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(ModelProfileEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(ModelProfileDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.modelProfile.delete(id).subscribe(() => this.loadData()); }
    });
  }
}
