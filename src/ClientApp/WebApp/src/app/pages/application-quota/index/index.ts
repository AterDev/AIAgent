import { Component, OnInit, signal, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ApplicationQuotaFilterDto } from 'src/app/services/admin/models/model-mod/application-quota-filter-dto.model';
import { ApplicationQuotaItemDto } from 'src/app/services/admin/models/model-mod/application-quota-item-dto.model';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';

@Component({
  selector: 'app-application-quota-index',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, EnumTextPipe],
  templateUrl: './index.html',
  standalone: true
})
export class ApplicationQuotaIndex implements OnInit {

  i18nKeys = I18N_KEYS;

  private adminClient = inject(AdminClient);
  private dialog = inject(MatDialog);
  private translate = inject(TranslateService);

  filterDto: ApplicationQuotaFilterDto = { pageIndex: 1, pageSize: 10 };
  dataSource = new MatTableDataSource<ApplicationQuotaItemDto>();
  displayedColumns = ["applicationId", "periodType", "maxRequests", "maxTokens", "isEnabled", "createdTime", "actions"];

  isLoading = signal(true);

  total = 0;
  pageSize = 10;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.applicationQuota.list(this.filterDto).subscribe({
      next: (res: any) => {
        this.dataSource.data = (res.data || []);
        this.total = (res.count ?? res.data?.length ?? this.dataSource.data.length);
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load quota data:', err);
        this.isLoading.set(false);
      }
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
    // TODO: Implement add dialog
    console.log('Add quota');
  }

  delete(id: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant('common.deleteConfirm')
      }
    });
    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) {
        this.adminClient.applicationQuota.delete(id).subscribe({
          next: () => {
            this.loadData();
          },
          error: (err: any) => {
            console.error('Failed to delete quota:', err);
          }
        });
      }
    });
  }

  viewUsage(row: ApplicationQuotaItemDto): void {
    this.adminClient.applicationQuota.getUsage(row.applicationId, row.periodType).subscribe({
      next: (res: any) => {
        const msg = `${this.translate.instant('applicationQuota.viewUsage')}: ${res.currentRequests}/${res.maxRequests} ${this.translate.instant('applicationQuota.maxRequests')}, ${res.currentTokens}/${res.maxTokens} ${this.translate.instant('applicationQuota.maxTokens')}`;
        alert(msg);
      },
      error: (err: any) => {
        console.error('Failed to load usage data:', err);
      }
    });
  }

  resetQuota(row: ApplicationQuotaItemDto): void {
    const periodName = row.periodType === 0 ? 'minute' : row.periodType === 1 ? 'hour' : row.periodType === 2 ? 'day' : 'month';
    const confirmMessage = `${this.translate.instant('applicationQuota.resetQuota')} (${this.translate.instant('applicationQuota.' + periodName)})?`;
    if (confirm(confirmMessage)) {
      this.adminClient.applicationQuota.resetQuota({
        applicationId: row.applicationId,
        periodType: row.periodType
      }).subscribe({
        next: () => {
          alert(this.translate.instant('common.saveSuccess'));
          this.loadData();
        },
        error: (err: any) => {
          console.error('Failed to reset quota:', err);
        }
      });
    }
  }
}
