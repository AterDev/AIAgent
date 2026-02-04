import { Component, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { StorageProviderAddDto } from 'src/app/services/admin/models/system-mod/storage-provider-add-dto.model';

@Component({
  selector: 'app-storage-provider-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class StorageProviderAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  isLoading = signal(false);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<StorageProviderAdd>,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.setupDynamicValidation();
  }

  ngOnInit(): void { }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(60)]],
      isCloud: [false, []],
      path: [null, [Validators.maxLength(200)]],
      endpoint: [null, [Validators.maxLength(200)]],
      accessKeyId: [null, [Validators.maxLength(100)]],
      accessKeySecret: [null, [Validators.maxLength(100)]],
      bucketName: [null, [Validators.maxLength(100)]],
      region: [null, [Validators.maxLength(100)]],
      isActive: [false, []]
    });
  }

  private setupDynamicValidation() {
    this.isCloud.valueChanges.subscribe(value => {
      if (value) {
        // 云存储：必填云配置字段
        this.endpoint.setValidators([Validators.required, Validators.maxLength(200)]);
        this.accessKeyId.setValidators([Validators.required, Validators.maxLength(100)]);
        this.accessKeySecret.setValidators([Validators.required, Validators.maxLength(100)]);
        this.bucketName.setValidators([Validators.required, Validators.maxLength(100)]);
        this.path.clearValidators();
      } else {
        // 本地存储：清除云配置验证
        this.endpoint.clearValidators();
        this.accessKeyId.clearValidators();
        this.accessKeySecret.clearValidators();
        this.bucketName.clearValidators();
        this.path.setValidators([Validators.maxLength(200)]);
      }
      this.endpoint.updateValueAndValidity();
      this.accessKeyId.updateValueAndValidity();
      this.accessKeySecret.updateValueAndValidity();
      this.bucketName.updateValueAndValidity();
      this.path.updateValueAndValidity();
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get isCloud() { return this.form.get('isCloud') as FormControl; }
  get path() { return this.form.get('path') as FormControl; }
  get endpoint() { return this.form.get('endpoint') as FormControl; }
  get accessKeyId() { return this.form.get('accessKeyId') as FormControl; }
  get accessKeySecret() { return this.form.get('accessKeySecret') as FormControl; }
  get bucketName() { return this.form.get('bucketName') as FormControl; }
  get region() { return this.form.get('region') as FormControl; }
  get isActive() { return this.form.get('isActive') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    this.adminClient.storageProvider.add(this.form.value as StorageProviderAddDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
