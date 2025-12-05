import { Component, OnInit, ViewChild, TemplateRef, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemUserDetailDto } from 'src/app/services/admin/models/system-mod/system-user-detail-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, forkJoin } from 'rxjs';
import { Location } from '@angular/common';
import { CommonListModules } from 'src/app/share/shared-modules';
import { MatCardModule } from '@angular/material/card';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GenderType } from 'src/app/services/admin/models/ater/gender-type.model';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-detail',
  imports: [...CommonListModules, MatCardModule, EnumTextPipe, TranslateModule],
  templateUrl: './detail.html',
  styleUrls: ['./detail.scss']
})
export class Detail implements OnInit {
  i18nKeys = I18N_KEYS;
  GenderType = GenderType;
  private adminClient = inject(AdminClient);

  readonly dlgData = inject(MAT_DIALOG_DATA);
  isLoading = signal(true);
  data = {} as SystemUserDetailDto;
  isProcessing = false;
  id: string = '';
  constructor(
    private snb: MatSnackBar,
    private route: ActivatedRoute,
    public location: Location,
    private router: Router,
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = this.dlgData.id;
    }
  }

  ngOnInit(): void {
    this.getDetail();
  }

  getDetail(): void {
    this.adminClient.systemUser.getDetail(this.id)
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

