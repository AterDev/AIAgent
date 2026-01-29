import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { BaseMatModules } from 'src/app/share/shared-modules';
import { AIAgentDetailDto } from 'src/app/services/admin/models/aiagent-mod/aiagent-detail-dto.model';

@Component({
  selector: 'app-aiagent-detail',
  imports: [BaseMatModules, MatListModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './detail.html',
  standalone: true
})
export class AIAgentDetail implements OnInit {

  i18nKeys = I18N_KEYS;

  model!: AIAgentDetailDto;
  id?: string;
  isLoading = signal(true);

  constructor(
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIAgentDetail>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.aIAgent.detail(this.id).subscribe({
        next: (res: AIAgentDetailDto) => {
          this.model = res;
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  close() { this.dialogRef.close(); }
}
