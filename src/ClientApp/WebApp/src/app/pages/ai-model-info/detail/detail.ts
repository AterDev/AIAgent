import { Component, OnInit, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AIModelInfoDetailDto } from 'src/app/services/admin/models/model-mod/aimodel-info-detail-dto.model';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-ai-model-info-detail',
  imports: [CommonModule, TranslateModule, MatButtonModule, MatIconModule, MatListModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './detail.html',
  standalone: true
})
export class AIModelInfoDetail implements OnInit {
  i18nKeys = I18N_KEYS;
  model: AIModelInfoDetailDto | null = null;
  isLoading = signal(false);
  id: string = '';

  constructor(
    private adminClient: AdminClient,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
      this.loadData();
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.aIModelInfo.detail(this.id).subscribe({
      next: (data: any) => {
        this.model = data;
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onBack(): void {
    this.router.navigate(['/ai-model-info/index']);
  }
}
