import { Component, OnInit, ViewChild, TemplateRef, inject, signal } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-mod/system-role-item-dto.model';
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
import { Menus } from '../menus/menus';
import { PageList } from 'src/app/services/admin/models/ater/page-list.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-index',
  imports: [...CommonListModules, ...CommonFormModules, TypedCellDefDirective],
  templateUrl: './index.html',
  styleUrls: ['./index.scss']
})
export class Index implements OnInit {
  i18nKeys = I18N_KEYS;
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = signal(true);
  isProcessing = false;
  total = 0;
  data: SystemRoleItemDto[] = [];
  columns: string[] = ['name', 'isSystem', 'createdTime', 'actions'];
  dataSource!: MatTableDataSource<SystemRoleItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('myDialog', { static: true }) myTmpl!: TemplateRef<{}>;
  mydialogForm!: FormGroup;
  filter = {
    pageIndex: 1,
    pageSize: 12,
    name: '',
    nameValue: ''
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
              this.dataSource = new MatTableDataSource<SystemRoleItemDto>(this.data);
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

  getListAsync(): Observable<PageList<SystemRoleItemDto>> {
    return this.adminClient.systemRole.list(this.filter.name, this.filter.nameValue, this.filter.pageIndex, this.filter.pageSize, null);
  }

  getList(event?: PageEvent): void {
    if (event) {
      this.filter.pageIndex = event.pageIndex + 1;
      this.filter.pageSize = event.pageSize;
    }
    this.adminClient.systemRole.list(this.filter.name, this.filter.nameValue, this.filter.pageIndex, this.filter.pageSize, null)
      .subscribe({
        next: (res) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<SystemRoleItemDto>(this.data);
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

  openMenuDialog(item: SystemRoleItemDto): void {
    this.dialogRef = this.dialog.open(Menus, {
      minWidth: '400px',
      data: { id: item.id }
    });
  }

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }

  openDetailDialog(item: SystemRoleItemDto): void {
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
  openEditDialog(item: SystemRoleItemDto): void {
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
  deleteConfirm(item: SystemRoleItemDto): void {
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
  delete(item: SystemRoleItemDto): void {
    this.isProcessing = true;
    this.adminClient.systemRole.delete(item.id)
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

