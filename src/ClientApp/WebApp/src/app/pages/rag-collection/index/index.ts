import { Component, OnInit, signal, inject } from '@angular/core';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { RagCollectionFilterDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-collection-filter-dto.model';
import { RagCollectionItemDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-collection-item-dto.model';
import { RagCollectionAdd } from '../add/add';
import { RagCollectionEdit } from '../edit/edit';
import { RagCollectionDetail } from '../detail/detail';
import { RagDocumentIndex } from '../../rag-document/index/index';
import { ApplicationRagCollectionPermissionItemDto } from 'src/app/services/admin/models/model-mod/application-rag-collection-permission-item-dto.model';

@Component({
  selector: 'app-rag-collection-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule],
  templateUrl: './index.html',
  standalone: true
})
export class RagCollectionIndex implements OnInit {
  i18nKeys = I18N_KEYS;
  filterDto: RagCollectionFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<RagCollectionItemDto>();
  displayedColumns = ["name", "isPublic", "isEnabled", "actions"];
  isLoading = signal(true);
  total = 0;
  pageSize = 10;
  applicationId?: string;
  applicationName?: string;
  private linkedPermissions = new Map<string, ApplicationRagCollectionPermissionItemDto>();
  private dialogData = inject(MAT_DIALOG_DATA, { optional: true }) as any;

  constructor(
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private translate: TranslateService,
    private snackBar: MatSnackBar
  ) {
    this.applicationId = this.dialogData?.applicationId;
    this.applicationName = this.dialogData?.applicationName;
    if (this.applicationId) {
      this.filterDto.applicationId = this.applicationId;
    }
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    const collection$ = this.adminClient.ragCollection.list(this.filterDto as RagCollectionFilterDto);
    const permission$ = this.applicationId
      ? this.adminClient.applicationRagCollectionPermission.list({ applicationId: this.applicationId, pageIndex: 1, pageSize: 500 })
      : null;

    (permission$ ? forkJoin({ collections: collection$, permissions: permission$ }) : forkJoin({ collections: collection$ }))
      .subscribe({
        next: (res: any) => {
          this.dataSource.data = (res.collections.data || []);
          this.total = (res.collections.count ?? res.collections.data?.length ?? this.dataSource.data.length);
          this.linkedPermissions.clear();
          for (const item of (res.permissions?.data || [])) {
            this.linkedPermissions.set(item.ragCollectionId, item);
          }
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
    const ref = this.dialog.open(RagCollectionAdd, { width: '800px', data: { applicationId: this.applicationId } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(RagCollectionEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(RagCollectionDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (!ok) {
        return;
      }

      if (this.applicationId) {
        const permission = this.linkedPermissions.get(id);
        if (!permission) {
          this.snackBar.open(this.translate.instant('common.deleteFail'), undefined, { duration: 2000 });
          return;
        }

        this.adminClient.applicationRagCollectionPermission.delete(permission.id).subscribe(() => this.loadData());
        return;
      }

      this.adminClient.ragCollection.delete(id).subscribe(() => this.loadData());
    });
  }

  openDocuments(item: RagCollectionItemDto): void {
    this.dialog.open(RagDocumentIndex, {
      width: '1100px',
      maxWidth: '96vw',
      data: {
        applicationId: this.applicationId,
        collectionId: item.id,
      }
    });
  }
}
