import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { BaseMatModules } from 'src/app/share/shared-modules';
import { WorkflowExecutionDetailDto } from 'src/app/services/admin/models/workflow-mod/workflow-execution-detail-dto.model';

@Component({
  selector: 'app-workflow-execution-detail',
  imports: [BaseMatModules, MatListModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './detail.html',
  standalone: true
})
export class WorkflowExecutionDetail implements OnInit {

  i18nKeys = I18N_KEYS;

  model!: WorkflowExecutionDetailDto;
  id?: string;
  isLoading = signal(true);

  constructor(
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<WorkflowExecutionDetail>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.adminClient.workflowExecution.detail(this.id).subscribe((res: WorkflowExecutionDetailDto) => {
        this.model = res;
        this.isLoading.set(false);
      });
    }
  }

  close() { this.dialogRef.close(); }
}
