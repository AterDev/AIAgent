import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';

@Component({
  selector: 'app-index',
  imports: [TranslateModule],
  templateUrl: './index.html',
  styleUrl: './index.scss'
})
export class Index {
  i18nKeys = I18N_KEYS;
}
