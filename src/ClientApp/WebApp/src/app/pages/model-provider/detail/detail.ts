import { Component, Inject, OnInit, signal } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { BaseMatModules } from 'src/app/share/shared-modules';
import { ModelProviderDetailDto } from 'src/app/services/admin/models/model-mod/model-provider-detail-dto.model';
import { ModelProviderType } from 'src/app/services/admin/models/entity/model-provider-type.model';

@Component({
  selector: 'app-model-provider-detail',
  imports: [BaseMatModules, MatListModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './detail.html',
  standalone: true
})
export class ModelProviderDetail implements OnInit {

  i18nKeys = I18N_KEYS;

  model!: ModelProviderDetailDto;
  id?: string;
  isLoading = signal(true);

  constructor(
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ModelProviderDetail>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.adminClient.modelProvider.detail(this.id).subscribe((res: ModelProviderDetailDto) => {
        this.model = res;
        this.isLoading.set(false);
      });
    }
  }

  getProviderTypeName(type: ModelProviderType): string {
    switch (type) {
      case ModelProviderType.OpenAiCompatible:
        return 'OpenAI Compatible';
      case ModelProviderType.Custom:
        return 'Custom';
      default:
        return 'Unknown';
    }
  }

  close() { this.dialogRef.close(); }
}
