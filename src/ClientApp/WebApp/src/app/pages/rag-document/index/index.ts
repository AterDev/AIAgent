import { Component, OnInit, signal, inject } from '@angular/core';  
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { RagDocumentFilterDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-filter-dto.model';
import { RagDocumentItemDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-item-dto.model';
import { RagCollectionItemDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-collection-item-dto.model';
import { RagDocumentAdd } from '../add/add';
import { RagDocumentEdit } from '../edit/edit';
import { RagDocumentDetail } from '../detail/detail';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';

@Component({
  selector: 'app-rag-document-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, EnumTextPipe],
  templateUrl: './index.html',
  standalone: true
})
export class RagDocumentIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  private dialog = inject(MatDialog);
  private translate = inject(TranslateService);
  private adminClient = inject(AdminClient);
  private snackBar = inject(MatSnackBar);

  filterDto: RagDocumentFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<RagDocumentItemDto>();
  displayedColumns = ["name", "collectionId", "status", "chunkCount", "actions"];

  isLoading = signal(true);
  availableCollections = signal<RagCollectionItemDto[]>([]);

  total = 0;
  pageSize = 10;

  ngOnInit(): void {
    this.loadCollections();
    this.loadData();
  }

  loadCollections(): void {
    this.adminClient.ragCollection.list({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (res: any) => {
        this.availableCollections.set(res.data || []);
      },
      error: () => {}
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.ragDocument.list(this.filterDto).subscribe({
      next: (res: any) => {
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
    const ref = this.dialog.open(RagDocumentAdd, { width: '800px' });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openEdit(id: string) {
    const ref = this.dialog.open(RagDocumentEdit, { width: '800px', data: { id } });
    ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
  }

  openDetail(id: string) {
    this.dialog.open(RagDocumentDetail, { minWidth: '600px', data: { id } });
  }

  deleteItem(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { 
        this.adminClient.ragDocument.delete(id).subscribe(() => this.loadData()); 
      }
    });
  }

  triggerParse(id: string) {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant(this.i18nKeys.ragDocument.triggerParseConfirm)
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) { 
        this.adminClient.ragDocument.triggerParse(id).subscribe({
          next: (res) => {
            this.snackBar.open(res.message || 'Parse triggered successfully', 'Close', { duration: 3000 });
            this.loadData();
          },
          error: (err) => {
            this.snackBar.open('Failed to trigger parse: ' + err.message, 'Close', { duration: 5000 });
          }
        }); 
      }
    });
  }

  getCollectionName(collectionId: string | null): string {
    if (!collectionId) return '';
    const collection = this.availableCollections().find(c => c.id === collectionId);
    return collection?.name || collectionId;
  }
}
