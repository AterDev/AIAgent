import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ModelProviderFilterDto } from 'src/app/services/admin/models/model-mod/model-provider-filter-dto.model';
import { ModelProviderItemDto } from 'src/app/services/admin/models/model-mod/model-provider-item-dto.model';
import { ModelProviderType } from 'src/app/services/admin/models/entity/model-provider-type.model';
import { ModelProviderAdd } from '../add/add.js';
import { ModelProviderEdit } from '../edit/edit.js';
import { ModelProviderDetail } from '../detail/detail.js';

@Component({
  selector: 'app-model-provider-index',
  imports: [CommonListModules, CommonFormModules],
  templateUrl: './index.html',
  standalone: true
})
export class ModelProviderIndex implements OnInit {

  i18nKeys = I18N_KEYS;
  ModelProviderType = ModelProviderType;

  filterDto: ModelProviderFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<ModelProviderItemDto>();
  displayedColumns = [ "name", "baseUrl", "providerType", "isEnabled", "actions" ];

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
    this.adminClient.modelProvider.list(this.filterDto as ModelProviderFilterDto).subscribe((res) => {
      this.dataSource.data = (res.data || []);
      this.total = (res.count ?? res.data?.length ?? this.dataSource.data.length);
    });
  }

  getProviderTypeName(type: ModelProviderType): string {
    switch (type) {
      case ModelProviderType.OpenAiCompatible:
        return 'OpenAI Compatible';
      case ModelProviderType.Custom:
        return 'Custom';
      default:
        return 'Unknown';
    }
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
    const ref = this.dialog.open(ModelProviderAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(ModelProviderEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(ModelProviderDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.modelProvider.delete(id).subscribe(() => this.loadData()); }
    });
  }
}
