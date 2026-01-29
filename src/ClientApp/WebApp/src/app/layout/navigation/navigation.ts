import { Component, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseMatModules, CommonModules } from 'src/app/share/shared-modules';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-navigation',
  imports: [...BaseMatModules, ...CommonModules, MatSidenavModule, MatExpansionModule, MatListModule, MatProgressSpinnerModule],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss'
})
export class NavigationComponent {
  i18nKeys = I18N_KEYS;
  events: string[] = [];
  opened = true;
  expanded = true;
  menus = signal<Menu[]>([]);
  isLoading = signal(false);
  constructor(
    private http: HttpClient,
  ) {
  }
  ngOnInit(): void {
    this.updateMenus();
  }

  toggle(): void {
    this.opened = !this.opened;
  }

  updateMenus(): void {
    this.isLoading.set(true);
    this.http.get<Menu[]>('/assets/menus.json?_t=' + Date.now(), { responseType: 'json' })
      .subscribe({
        next: (res) => {
          this.menus.set(res.sort((a, b) => a.sort - b.sort));
          this.isLoading.set(false);

          // const userMenus = JSON.parse(localStorage.getItem('menus') ?? 'null') ?? [];
          // const userMenuCodes = userMenus.map((item: any) => item.accessCode);
          // this.menus = this.mergeMenu(userMenuCodes, this.menus);
        }
        ,
        error: () => this.isLoading.set(false)
      });
  }
  mergeMenu(userMenuCodes: string[], menus: Menu[]): Menu[] {
    // 只保留有权限的菜单
    return menus.filter((item) => {
      if (userMenuCodes.includes(item.accessCode)) {
        if (item.children) {
          item.children = this.mergeMenu(userMenuCodes, item.children);
        }
        return true;
      }
      return false;
    });
  }
}
export interface Menu {
  name: string,
  path: string | null,
  accessCode: string,
  icon: string,
  sort: number,
  menuType: 0 | 1,
  children?: Menu[] | null,
}
