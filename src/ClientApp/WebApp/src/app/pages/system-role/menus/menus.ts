import { Component, Inject, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { SystemMenu } from 'src/app/services/admin/models/entity/system-menu.model';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { Observable, forkJoin } from 'rxjs';
import { MatTreeModule, MatTreeNestedDataSource } from '@angular/material/tree';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { CommonModules } from 'src/app/share/shared-modules';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { PageList } from 'src/app/services/admin/models/ater/page-list.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-menus',
  imports: [CommonModules, MatTreeModule, MatCheckboxModule, MatButtonModule, MatCardModule, TranslateModule],
  templateUrl: './menus.html',
  styleUrl: './menus.scss'
})
export class Menus {
  i18nKeys = I18N_KEYS;
  id!: string;
  isLoading = signal(true);
  data: SystemMenu[] = [];
  menuNods = [] as MenuNode[];
  dataSource = new MatTreeNestedDataSource<MenuNode>();
  selectedIds = [] as string[];
  private adminClient = inject(AdminClient);

  constructor(
    private route: ActivatedRoute,
    private snb: MatSnackBar,
    public dialogRef: MatDialogRef<Menus>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' },
    private translate: TranslateService
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

  childrenAccessor = (node: MenuNode) => node.children ?? [];
  hasChild = (_: number, node: MenuNode) => !!node.children && node.children.length > 0;

  transformMenuToNode(menu: SystemMenu[], checkedMenus: SystemMenu[]): MenuNode[] {
    return menu.map((item) => {
      return {
        id: item.id,
        name: item.name,
        children: item.children ? this.transformMenuToNode(item.children, checkedMenus) : null,
        checked: checkedMenus.some(checkedMenu => checkedMenu.id === item.id)
      };
    });
  }

  ngOnInit(): void {
    forkJoin([this.getMemusAsync(), this.getRoleMenusAsync()])
      .subscribe({
        next: ([menus, roleMenus]) => {
          if (menus.data && roleMenus.data) {
            this.menuNods = this.transformMenuToNode(menus.data, roleMenus.data);
            this.dataSource.data = this.menuNods;
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

  getMemusAsync(): Observable<PageList<SystemMenu>> {
    return this.adminClient.systemMenu.filter({ pageIndex: 1, pageSize: 999 });
  }

  getRoleMenusAsync(): Observable<PageList<SystemMenu>> {
    return this.adminClient.systemMenu.filter({
      pageIndex: 1,
      pageSize: 100,
      roleId: this.id
    });
  }

  onCheckboxChange(node: MenuNode, event: any): void {
    node.checked = event.checked
  }

  onParentNodeChange(node: MenuNode, event: MatCheckboxChange) {
    node.children?.forEach(child => {
      child.checked = event.checked;
    });
  }

  isSelectAll(node: MenuNode) {
    if (node.children) {
      return node.children.every(child => child.checked === true);
    }
    return false;
  }

  hasSelected(node: MenuNode) {
    if (node.children) {
      if (node.children.every(child => child.checked === true)) {
        return false;
      }
      return node.children.some(child => child.checked === true);
    }
    return false;
  }

  getSelectedMenuIds(menus: MenuNode[]): void {
    for (const element of menus) {
      if (element.checked) {
        this.selectedIds.push(element.id);
      }
      if (element.children) {
        this.getSelectedMenuIds(element.children);
      }
    }
  }

  save(): void {
    this.selectedIds = [];
    this.getSelectedMenuIds(this.menuNods);

    this.adminClient.systemRole.updateMenus({
      id: this.id,
      menuIds: this.selectedIds
    }).subscribe({
      next: (res) => {
        if (res) {
          this.snb.open(this.translate.instant(this.i18nKeys.common.saveSuccess));
          this.dialogRef.close(res);
        }
      },
      error: (error) => {
        this.snb.open(error.detail);
      }
    });
  }
}


interface MenuNode {
  id: string;
  name: string;
  children: MenuNode[] | null;
  checked: boolean;
}
