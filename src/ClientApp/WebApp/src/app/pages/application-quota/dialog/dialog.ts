import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ApplicationQuotaFilterDto } from 'src/app/services/admin/models/model-mod/application-quota-filter-dto.model';
import { ApplicationQuotaItemDto } from 'src/app/services/admin/models/model-mod/application-quota-item-dto.model';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';
import { QuotaUsageDto } from 'src/app/services/admin/models/model-mod/quota-usage-dto.model';
import { ApplicationQuotaAdd } from '../add/add';
import { ApplicationQuotaEdit } from '../edit/edit';

export interface ApplicationQuotaDialogData {
  applicationId: string;
  applicationName: string;
}

@Component({
  selector: 'app-application-quota-dialog',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, EnumTextPipe, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './dialog.html',
  standalone: true
})
export class ApplicationQuotaDialog implements OnInit {

  i18nKeys = I18N_KEYS;

  filterDto: ApplicationQuotaFilterDto;
  dataSource = new MatTableDataSource<ApplicationQuotaItemDto>();
  displayedColumns = ['periodType', 'maxRequests', 'maxTokens', 'windowSeconds', 'isEnabled', 'actions'];

  isLoading = signal(true);
  total = 0;

  constructor(
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<ApplicationQuotaDialog>,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationQuotaDialogData
  ) {
    this.filterDto = {
      applicationId: data.applicationId,
      pageIndex: 1,
      pageSize: 10,
    };
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.applicationQuota.list(this.filterDto).subscribe({
      next: (res) => {
        this.dataSource.data = res.data || [];
        this.total = res.count ?? this.dataSource.data.length;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  pageChanged(e: any) {
    this.filterDto.pageIndex = e.pageIndex + 1;
    this.filterDto.pageSize = e.pageSize;
    this.loadData();
  }

  openAdd() {
    const ref = this.dialog.open(ApplicationQuotaAdd, {
      width: '720px',
      data: {
        applicationId: this.data.applicationId,
        applicationName: this.data.applicationName,
      }
    });

    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) {
        this.loadData();
      }
    });
  }

  openEdit(row: ApplicationQuotaItemDto) {
    const ref = this.dialog.open(ApplicationQuotaEdit, {
      width: '720px',
      data: {
        id: row.id,
        applicationId: this.data.applicationId,
        applicationName: this.data.applicationName,
      }
    });

    ref.afterClosed().subscribe((ok: boolean) => {
      if (ok) {
        this.loadData();
      }
    });
  }

  delete(row: ApplicationQuotaItemDto): void {
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

      this.adminClient.applicationQuota.delete(row.id).subscribe(() => this.loadData());
    });
  }

  viewUsage(row: ApplicationQuotaItemDto): void {
    this.adminClient.applicationQuota.getUsage(this.data.applicationId, row.periodType).subscribe({
      next: (usage) => this.showUsageMessage(usage),
    });
  }

  resetQuota(row: ApplicationQuotaItemDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant(this.i18nKeys.applicationQuota.resetQuotaConfirm),
      }
    });

    ref.afterClosed().subscribe((ok: boolean) => {
      if (!ok) {
        return;
      }

      this.adminClient.applicationQuota.resetQuota({
        applicationId: this.data.applicationId,
        periodType: row.periodType,
      }).subscribe(() => {
        this.snackBar.open(this.translate.instant('common.saveSuccess'), undefined, { duration: 2000 });
      });
    });
  }

  close() {
    this.dialogRef.close();
  }

  private showUsageMessage(usage: QuotaUsageDto) {
    const windowText = `${usage.windowStart} ~ ${usage.windowEnd}`;
    const message = `${this.translate.instant(this.i18nKeys.applicationQuota.currentRequests)}: ${usage.currentRequests}/${usage.maxRequests} · ${this.translate.instant(this.i18nKeys.applicationQuota.currentTokens)}: ${usage.currentTokens}/${usage.maxTokens} · ${windowText}`;
    this.snackBar.open(message, this.translate.instant(this.i18nKeys.common.close), { duration: 5000 });
  }
}