import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { StorageProviderFilterDto } from 'src/app/services/admin/models/system-mod/storage-provider-filter-dto.model';
import { StorageProviderItemDto } from 'src/app/services/admin/models/system-mod/storage-provider-item-dto.model';
import { StorageProviderAdd } from '../add/add';
import { StorageProviderEdit } from '../edit/edit';
import { StorageProviderDetail } from '../detail/detail';

@Component({
  selector: 'app-storage-provider-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule],
  templateUrl: './index.html',
  standalone: true
})
export class StorageProviderIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: StorageProviderFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<StorageProviderItemDto>();
  displayedColumns = ["name", "isCloud", "bucketName", "region", "isActive", "actions"];

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
    this.adminClient.storageProvider.list(this.filterDto as StorageProviderFilterDto).subscribe({
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
    const ref = this.dialog.open(StorageProviderAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(StorageProviderEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(StorageProviderDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.storageProvider.delete(id).subscribe(() => this.loadData()); }
    });
  }

  activateItem(id: string) {
    this.adminClient.storageProvider.activate(id).subscribe(() => this.loadData());
  }
}
