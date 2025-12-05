import { Component, OnInit, ViewChild, TemplateRef, Inject, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Location } from '@angular/common';
import { CommonListModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { SystemRoleDetailDto } from 'src/app/services/admin/models/system-mod/system-role-detail-dto.model';
import { AdminClient } from 'src/app/services/admin/admin-client';

@Component({
  selector: 'app-detail',
  imports: [...CommonListModules, MatCardModule, TranslateModule],
  templateUrl: './detail.html',
  styleUrls: ['./detail.scss']
})
export class Detail implements OnInit {
  i18nKeys = I18N_KEYS;
  isLoading = signal(true);
  data = {} as SystemRoleDetailDto;
  isProcessing = false;
  id: string = '';
  private adminClient = inject(AdminClient);

  constructor(
    private snb: MatSnackBar,
    private route: ActivatedRoute,
    public location: Location,
    private router: Router,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }

  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

  ngOnInit(): void {
    this.getDetail();
  }

  getDetail(): void {
    this.adminClient.systemRole.detail(this.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = res;
            this.isLoading.set(false);
          }
        },
        error: (error) => {
          this.snb.open(error);
        }
      })
  }
  back(): void {
    this.location.back();
  }

  edit(): void {
  }
}
