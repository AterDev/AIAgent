import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';
import { ApplicationApiKeyCredentialResultDto } from 'src/app/services/admin/models/model-mod/application-api-key-credential-result-dto.model';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-application-secret-dialog',
  standalone: true,
  imports: [CommonFormModules, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './secret-dialog.html'
})
export class ApplicationSecretDialog {
  i18nKeys = I18N_KEYS;

  constructor(
    private dialogRef: MatDialogRef<ApplicationSecretDialog>,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: ApplicationApiKeyCredentialResultDto
  ) { }

  async copy(value: string, messageKey: string) {
    try {
      await navigator.clipboard.writeText(value);
      this.snackBar.open(this.translate.instant(messageKey), undefined, { duration: 2000 });
    } catch {
      this.snackBar.open(this.translate.instant(this.i18nKeys.application.secretOnlyVisibleOnce), undefined, { duration: 3000 });
    }
  }

  close() {
    this.dialogRef.close();
  }
}