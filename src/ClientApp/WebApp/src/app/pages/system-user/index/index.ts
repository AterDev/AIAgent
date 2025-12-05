import { Component, OnInit, ViewChild, TemplateRef, inject, signal } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { AdminClient } from 'src/app/services/admin/admin-client';

import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { CommonFormModules, CommonListModules } from 'src/app/share/shared-modules';
import { TypedCellDefDirective } from 'src/app/share/typed-cell-def.directive';
import { Detail } from '../detail/detail';

import { Add } from '../add/add';
import { Edit } from '../edit/edit';
import { EnumTextPipe } from 'src/app/pipe/admin/enum-text.pipe';
import { PageList } from 'src/app/services/admin/models/ater/page-list.model';
import { SystemUserItemDto } from 'src/app/services/admin/models/system-mod/system-user-item-dto.model';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-index',
  imports: [...CommonListModules, ...CommonFormModules, TypedCellDefDirective, EnumTextPipe],
  templateUrl: './index.html',
  styleUrls: ['./index.scss']
})
export class Index implements OnInit {
  i18nKeys = I18N_KEYS;

  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = signal(true);
  isProcessing = false;
  total = 0;
  data: SystemUserItemDto[] = [];
  columns: string[] = ['userName', 'realName', 'email', 'lastLoginTime', 'sex', 'createdTime', 'actions'];
  dataSource!: MatTableDataSource<SystemUserItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('myDialog', { static: true }) myTmpl!: TemplateRef<{}>;
  mydialogForm!: FormGroup;
  filter = {
    pageIndex: 1,
    pageSize: 12,
    userName: '',
    roleId: ''
  };
  pageSizeOption = [12, 20, 50];
  private adminClient = inject(AdminClient);

  constructor(
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
  ) {
  }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
      .subscribe({
        next: ([res]) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<SystemUserItemDto>(this.data);
            }
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading.set(false);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
  }

  getListAsync(): Observable<PageList<SystemUserItemDto>> {
    return this.adminClient.systemUser.filter(this.filter.userName, this.filter.roleId, this.filter.pageIndex, this.filter.pageSize, null);
  }

  getList(event?: PageEvent): void {
    if (event) {
      this.filter.pageIndex = event.pageIndex + 1;
      this.filter.pageSize = event.pageSize;
    }
    this.adminClient.systemUser.filter(this.filter.userName, this.filter.roleId, this.filter.pageIndex, this.filter.pageSize, null)
      .subscribe({
        next: (res) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<SystemUserItemDto>(this.data);
            }
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading.set(false);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
  }

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }

  openDetailDialog(item: SystemUserItemDto): void {
    this.dialogRef = this.dialog.open(Detail, {
      minWidth: '400px',
      maxHeight: '98vh',
      data: { id: item.id }
    });
  }


  openAddDialog(): void {
    this.dialogRef = this.dialog.open(Add, {
      minWidth: '400px',
      maxHeight: '98vh'
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }
  openEditDialog(item: SystemUserItemDto): void {
    this.dialogRef = this.dialog.open(Edit, {
      minWidth: '400px',
      maxHeight: '98vh',
      data: { id: item.id }
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }
  deleteConfirm(item: SystemUserItemDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      data: {
        title: this.translate.instant(this.i18nKeys.common.delete),
        content: this.translate.instant(this.i18nKeys.common.confirmDelete)
      }
    });

    ref.afterClosed().subscribe(res => {
      if (res) {
        this.delete(item);
      }
    });
  }
  delete(item: SystemUserItemDto): void {
    this.isProcessing = true;
    this.adminClient.systemUser.delete(item.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = this.data.filter(_ => _.id !== item.id);
            this.dataSource.data = this.data;
            this.snb.open(this.translate.instant(this.i18nKeys.common.deleteSuccess));
          } else {
            this.snb.open(this.translate.instant(this.i18nKeys.common.deleteFail));
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
  }

  /**
   * 编辑
   */
  edit(id: string): void {
    this.router.navigate(['../edit/' + id], { relativeTo: this.route });
  }
}

