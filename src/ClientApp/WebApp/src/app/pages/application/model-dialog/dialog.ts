import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { AIModelInfoItemDto } from 'src/app/services/admin/models/model-mod/aimodel-info-item-dto.model';
import { ApplicationModelPermissionItemDto } from 'src/app/services/admin/models/model-mod/application-model-permission-item-dto.model';
import { ApplicationModelPermissionSyncDto } from 'src/app/services/admin/models/model-mod/application-model-permission-sync-dto.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';

export interface ApplicationModelPermissionDialogData {
  applicationId: string;
  applicationName: string;
}

@Component({
  selector: 'app-application-model-permission-dialog',
  imports: [CommonListModules, CommonFormModules, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './dialog.html',
  standalone: true
})
export class ApplicationModelPermissionDialog implements OnInit {
  i18nKeys = I18N_KEYS;
  displayedColumns = ['selected', 'name', 'supportsChat', 'supportsEmbedding'];
  dataSource = new MatTableDataSource<AIModelInfoItemDto>();
  isLoading = signal(true);
  private permissionMap = new Map<string, ApplicationModelPermissionItemDto>();
  private selectedModelIds = new Set<string>();

  constructor(
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ApplicationModelPermissionDialog>,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationModelPermissionDialogData
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    forkJoin({
      models: this.adminClient.aIModelInfo.list({ pageIndex: 1, pageSize: 200 }),
      permissions: this.adminClient.applicationModelPermission.list({ applicationId: this.data.applicationId, pageIndex: 1, pageSize: 500 })
    }).subscribe({
      next: ({ models, permissions }) => {
        this.dataSource.data = models.data || [];
        this.permissionMap.clear();
        this.selectedModelIds.clear();
        for (const item of (permissions.data || [])) {
          this.permissionMap.set(item.aiModelInfoId, item);
          this.selectedModelIds.add(item.aiModelInfoId);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  hasPermission(modelId: string): boolean {
    return this.selectedModelIds.has(modelId);
  }

  toggleSelection(modelId: string, checked: boolean): void {
    if (checked) {
      this.selectedModelIds.add(modelId);
      return;
    }

    this.selectedModelIds.delete(modelId);
  }

  submit(): void {
    this.isLoading.set(true);
    const payload: ApplicationModelPermissionSyncDto = {
      applicationId: this.data.applicationId,
      aiModelInfoIds: Array.from(this.selectedModelIds)
    };

    this.adminClient.applicationModelPermission.sync(payload).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('common.saveSuccess'), undefined, { duration: 2000 });
        this.dialogRef.close(true);
      },
      error: () => {
        this.isLoading.set(false);
        this.snackBar.open(this.translate.instant('common.saveFail'), undefined, { duration: 2000 });
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}