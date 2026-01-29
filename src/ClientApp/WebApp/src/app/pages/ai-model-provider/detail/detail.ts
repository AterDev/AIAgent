import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { BaseMatModules } from 'src/app/share/shared-modules';
import { AIModelProviderDetailDto } from 'src/app/services/admin/models/model-mod/aimodel-provider-detail-dto.model';

@Component({
  selector: 'app-ai-model-provider-detail',
  imports: [BaseMatModules, MatListModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './detail.html',
  standalone: true
})
export class AIModelProviderDetail implements OnInit {

  i18nKeys = I18N_KEYS;

  model!: AIModelProviderDetailDto;
  id?: string;
  isLoading = signal(true);

  constructor(
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<AIModelProviderDetail>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.adminClient.aIModelProvider.detail(this.id).subscribe((res: AIModelProviderDetailDto) => {
        this.model = res;
        this.isLoading.set(false);
      });
    }
  }

  close() { this.dialogRef.close(); }
}
