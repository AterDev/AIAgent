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
import { RagDocumentAddDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-add-dto.model';
import { RagDocumentStatus } from 'src/app/services/admin/models/entity/rag-document-status.model';

@Component({
  selector: 'app-rag-document-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class RagDocumentAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  isLoading = signal(true);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<RagDocumentAdd>,
    private translate: TranslateService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
  }

  buildForm() {
    this.form = this.fb.group({
      collectionId: [null, [Validators.required]],
      name: [null, [Validators.required, Validators.maxLength(200)]],
      fileName: [null, [Validators.maxLength(260)]],
      contentType: [null, [Validators.maxLength(100)]],
      status: [RagDocumentStatus.Pending, [Validators.required]],
      tags: [[], []],
      roles: [[], []],
      sourceUrl: [null, [Validators.maxLength(500)]]
    });
  }

  get collectionId() { return this.form.get('collectionId') as FormControl; }
  get name() { return this.form.get('name') as FormControl; }
  get fileName() { return this.form.get('fileName') as FormControl; }
  get contentType() { return this.form.get('contentType') as FormControl; }
  get status() { return this.form.get('status') as FormControl; }
  get tags() { return this.form.get('tags') as FormControl; }
  get roles() { return this.form.get('roles') as FormControl; }
  get sourceUrl() { return this.form.get('sourceUrl') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  submit() {
    if (this.form.invalid) return;
    this.adminClient.ragDocument.add(this.form.value as RagDocumentAddDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
