import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AIModelInfoFilterDto } from 'src/app/services/admin/models/model-mod/aimodel-info-filter-dto.model';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { CommonListModules } from 'src/app/share/shared-modules';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-ai-model-info-index',
    imports: [
        CommonListModules,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        MatPaginatorModule,
        MatProgressSpinnerModule,
        CommonModule,
        FormsModule,
        TranslateModule
    ],
    templateUrl: './index.html',
    standalone: true
})
export class AIModelInfoIndex implements OnInit {
    i18nKeys = I18N_KEYS;

    displayedColumns = ['name', 'displayName', 'contextLength', 'supportsChat', 'createdTime', 'actions'];
    dataSource: AIModelInfoItemDto[] = [];
    filterDto: AIModelInfoFilterDto = { pageIndex: 1, pageSize: 10 };
    isLoading = signal(false);
    total = 0;

    constructor(
        private adminClient: AdminClient,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadData();
    }

    loadData(): void {
        this.isLoading.set(true);
        this.adminClient.aIModelInfo.list(this.filterDto).subscribe({
            next: (res: any) => {
                this.dataSource = res.items || [];
                this.total = (res.items || []).length;
                this.isLoading.set(false);
            },
            error: () => this.isLoading.set(false)
        });
    }

    onPageChange(event: any): void {
        this.filterDto.pageIndex = event.pageIndex + 1;
        this.filterDto.pageSize = event.pageSize;
        this.loadData();
    }

    onAdd(): void {
        this.router.navigate(['/ai-model-info/add']);
    }

    onEdit(id: string): void {
        this.router.navigate(['/ai-model-info/edit', id]);
    }

    onDetail(id: string): void {
        this.router.navigate(['/ai-model-info/detail', id]);
    }

    onDelete(id: string): void {
        if (confirm(this.i18nKeys.common.deleteConfirm as any)) {
            this.adminClient.aIModelInfo.delete(id).subscribe(() => {
                this.loadData();
            });
        }
    }
}
