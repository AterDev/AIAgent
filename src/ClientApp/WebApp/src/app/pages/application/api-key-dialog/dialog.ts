import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ApplicationApiKeyItemDto } from 'src/app/services/admin/models/model-mod/application-api-key-item-dto.model';
import { ApplicationApiKeyCredentialResultDto } from 'src/app/services/admin/models/model-mod/application-api-key-credential-result-dto.model';
import { ApplicationApiKeyAdd } from '../api-key-add/add';
import { ApplicationSecretDialog } from '../secret-dialog/secret-dialog';

export interface ApplicationApiKeyDialogData {
  applicationId: string;
  applicationName: string;
}

@Component({
  selector: 'app-application-api-key-dialog',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './dialog.html',
  standalone: true
})
export class ApplicationApiKeyDialog implements OnInit {
  i18nKeys = I18N_KEYS;

  dataSource = new MatTableDataSource<ApplicationApiKeyItemDto>();
  displayedColumns = ['name', 'keyExpiresAt', 'createdTime', 'actions'];
  isLoading = signal(true);

  constructor(
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<ApplicationApiKeyDialog>,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationApiKeyDialogData
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.application.listApiKeys(this.data.applicationId).subscribe({
      next: (res) => {
        this.dataSource.data = res ?? [];
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openAdd(): void {
    const ref = this.dialog.open(ApplicationApiKeyAdd, {
      width: '720px',
      data: {
        applicationId: this.data.applicationId,
        applicationName: this.data.applicationName,
      }
    });

    ref.afterClosed().subscribe((result?: ApplicationApiKeyCredentialResultDto) => {
      if (!result) {
        return;
      }

      this.loadData();
      this.dialog.open(ApplicationSecretDialog, {
        width: '720px',
        data: result,
      });
    });
  }

  delete(row: ApplicationApiKeyItemDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant(this.i18nKeys.application.deleteApiKeyConfirm),
      }
    });

    ref.afterClosed().subscribe((ok: boolean) => {
      if (!ok) {
        return;
      }

      this.adminClient.application.deleteApiKey(this.data.applicationId, row.id).subscribe(() => this.loadData());
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}