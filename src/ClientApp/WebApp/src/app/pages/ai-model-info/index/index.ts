import { Component, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AIModelInfoFilterDto } from 'src/app/services/admin/models/model-mod/aimodel-info-filter-dto.model';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { CommonListModules, CommonFormModules } from 'src/app/share/shared-modules';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { AIModelInfoAdd } from '../add/add';
import { AIModelInfoEdit } from '../edit/edit';
import { AIModelInfoDetail } from '../detail/detail';

@Component({
    selector: 'app-ai-model-info-index',
    imports: [CommonListModules, CommonFormModules],
    templateUrl: './index.html',
    standalone: true
})
export class AIModelInfoIndex implements OnInit {
    i18nKeys = I18N_KEYS;

    displayedColumns = ['name', 'displayName', 'contextLength', 'supportsChat', 'createdTime', 'actions'];
    dataSource = new MatTableDataSource<AIModelInfoItemDto>();
    filterDto: AIModelInfoFilterDto = { pageIndex: 1, pageSize: 10 };
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
        this.adminClient.aIModelInfo.list(this.filterDto as AIModelInfoFilterDto).subscribe((res: any) => {
            this.dataSource.data = (res.data || res.items || []);
            this.total = (res.count || res.total || this.dataSource.data.length);
            this.isLoading.set(false);
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
        const ref = this.dialog.open(AIModelInfoAdd, { width: '800px' });
        ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
    }

    openEdit(id: string) {
        const ref = this.dialog.open(AIModelInfoEdit, { width: '800px', data: { id } });
        ref.afterClosed().subscribe((r: boolean) => { if (r) this.loadData(); });
    }

    openDetail(id: string) {
        this.dialog.open(AIModelInfoDetail, { minWidth: '600px', data: { id } });
    }

    deleteItem(id: string) {
        const ref = this.dialog.open(ConfirmDialogComponent, {
            data: {
                title: this.translate.instant('common.confirm'),
                content: this.translate.instant('common.deleteConfirm')
            }
        });
        ref.afterClosed().subscribe((ok: boolean) => {
            if (ok) { this.adminClient.aIModelInfo.delete(id).subscribe(() => this.loadData()); }
        });
    }
}
