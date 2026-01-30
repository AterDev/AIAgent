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
import { RagDocumentUpdateDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-update-dto.model';
import { RagDocumentDetailDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-detail-dto.model';

@Component({
  selector: 'app-rag-document-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class RagDocumentEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(true);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<RagDocumentEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.ragDocument.detail(this.id).subscribe({
        next: (res: RagDocumentDetailDto) => {
          this.form.patchValue(res);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  buildForm() {
    this.form = this.fb.group({
      "collectionId": [null, [Validators.required]],
      "name": [null, [Validators.required, Validators.maxLength(200)]],
      "fileName": [null, [Validators.maxLength(260)]],
      "contentType": [null, [Validators.maxLength(100)]],
      "status": [null, [Validators.required]],
      "tags": [null, []],
      "roles": [null, []],
      "sourceUrl": [null, [Validators.maxLength(500)]]
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
    if (!this.id) return;
    this.adminClient.ragDocument.update(this.id, this.form.value as RagDocumentUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
