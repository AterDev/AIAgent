import { Component, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { ApplicationAddDto } from 'src/app/services/admin/models/model-mod/application-add-dto.model';
import { ApplicationCredentialResultDto } from 'src/app/services/admin/models/model-mod/application-credential-result-dto.model';

@Component({
  selector: 'app-application-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class ApplicationAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  isLoading = signal(true);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<ApplicationAdd>,
    private translate: TranslateService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.isLoading.set(false);
  }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(100)]],
      description: [null, [Validators.maxLength(500)]],
      isEnabled: [true, []]
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
    this.adminClient.application.add(this.form.value as ApplicationAddDto)
      .subscribe((res: ApplicationCredentialResultDto) => this.dialogRef.close(res));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
