import { Component, Inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { RagCollectionAddDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-collection-add-dto.model';

@Component({
  selector: 'app-rag-collection-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class RagCollectionAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  isLoading = signal(true);
  applicationId?: string;

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<RagCollectionAdd>,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.buildForm();
    this.applicationId = data?.applicationId;
  }

  ngOnInit(): void {

    this.isLoading.set(false);
  }

  buildForm() {
    this.form = this.fb.group({
      name: [null, [Validators.required, Validators.maxLength(200)]],
      description: [null, [Validators.maxLength(1000)]],
      isPublic: [false, []],
      isEnabled: [true, []],
      tags: [[], []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get isPublic() { return this.form.get('isPublic') as FormControl; }
  get isEnabled() { return this.form.get('isEnabled') as FormControl; }
  get tags() { return this.form.get('tags') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    const payload: RagCollectionAddDto = {
      ...(this.form.value as RagCollectionAddDto),
      applicationId: this.applicationId ?? null,
    };

    this.adminClient.ragCollection.add(payload).subscribe({
      next: () => this.dialogRef.close(true)
    });
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
