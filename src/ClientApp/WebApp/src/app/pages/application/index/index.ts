import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ApplicationFilterDto } from 'src/app/services/admin/models/model-mod/application-filter-dto.model';
import { ApplicationItemDto } from 'src/app/services/admin/models/model-mod/application-item-dto.model';
import { ApplicationAdd } from '../add/add';
import { ApplicationEdit } from '../edit/edit';
import { ApplicationDetail } from '../detail/detail';
import { ApplicationQuotaDialog } from '../../application-quota/dialog/dialog';
import { ApplicationApiKeyDialog } from '../api-key-dialog/dialog';
import { ApplicationModelPermissionDialog } from '../model-dialog/dialog';
import { AIAgentIndex } from '../../aiagent/index/index';
import { RagCollectionIndex } from '../../rag-collection/index/index';

@Component({
  selector: 'app-application-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule],
  templateUrl: './index.html',
  standalone: true
})
export class ApplicationIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: ApplicationFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<ApplicationItemDto>();
  displayedColumns = [ 'name', 'isEnabled', 'createdTime', 'actions' ];

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
    this.adminClient.application.list(this.filterDto as ApplicationFilterDto).subscribe({
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
    const ref = this.dialog.open(ApplicationAdd, { width: '800px' });
    ref.afterClosed().subscribe((ok: boolean) => { if (ok) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(ApplicationEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(ApplicationDetail, { minWidth: '600px', data: { id } });
  }

  openQuota(item: ApplicationItemDto) {
    this.dialog.open(ApplicationQuotaDialog, {
      width: '960px',
      maxWidth: '96vw',
      data: {
        applicationId: item.id,
        applicationName: item.name,
      }
    });
  }

  openApiKeys(item: ApplicationItemDto) {
    this.dialog.open(ApplicationApiKeyDialog, {
      width: '960px',
      maxWidth: '96vw',
      data: {
        applicationId: item.id,
        applicationName: item.name,
      }
    });
  }

  openModels(item: ApplicationItemDto) {
    this.dialog.open(ApplicationModelPermissionDialog, {
      width: '1100px',
      maxWidth: '96vw',
      data: {
        applicationId: item.id,
        applicationName: item.name,
      }
    });
  }

  openAgents(item: ApplicationItemDto) {
    this.dialog.open(AIAgentIndex, {
      width: '1100px',
      maxWidth: '96vw',
      data: {
        applicationId: item.id,
        applicationName: item.name,
      }
    });
  }

  openKnowledge(item: ApplicationItemDto) {
    this.dialog.open(RagCollectionIndex, {
      width: '1100px',
      maxWidth: '96vw',
      data: {
        applicationId: item.id,
        applicationName: item.name,
      }
    });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { this.adminClient.application.delete(id).subscribe(() => this.loadData()); }
    });
  }
}
