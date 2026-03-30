import { Component, Inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ApplicationUpdateDto } from 'src/app/services/admin/models/model-mod/application-update-dto.model';
import { ApplicationDetailDto } from 'src/app/services/admin/models/model-mod/application-detail-dto.model';
import { ApplicationCredentialResultDto } from 'src/app/services/admin/models/model-mod/application-credential-result-dto.model';
import { ConfirmDialogComponent } from 'src/app/share/components/confirm-dialog/confirm-dialog.component';
import { ApplicationSecretDialog } from '../secret-dialog/secret-dialog';

@Component({
  selector: 'app-application-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class ApplicationEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);
  clientIdValue = signal('');

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<ApplicationEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.application.detail(this.id).subscribe({
        next: (res: ApplicationDetailDto) => {
          this.form.patchValue({
            name: res.name,
            description: res.description,
            isEnabled: res.isEnabled,
          });
          this.clientIdValue.set(res.clientId);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "name": [null, [Validators.required, Validators.maxLength(100)]],
      "description": [null, [Validators.maxLength(500)]],
      "isEnabled": [null, []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get isEnabled() { return this.form.get('isEnabled') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    if (!this.id) return;
    this.adminClient.application.update(this.id, this.form.value as ApplicationUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  resetSecret() {
    if (!this.id) return;

    this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirm'),
        content: this.translate.instant(this.i18nKeys.application.resetSecretConfirm),
      }
    }).afterClosed().subscribe((ok: boolean) => {
      if (!ok) return;

      this.adminClient.application.resetSecret(this.id!)
        .subscribe((res: ApplicationCredentialResultDto) => {
          this.dialog.open(ApplicationSecretDialog, {
            width: '720px',
            data: res,
          });
        });
    });
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
