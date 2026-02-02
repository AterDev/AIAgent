import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { RagDocumentAddDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-document-add-dto.model';
import { RagDocumentStatus } from 'src/app/services/admin/models/entity/rag-document-status.model';
import { RagCollectionItemDto } from 'src/app/services/admin/models/knowledge-base-mod/rag-collection-item-dto.model';

@Component({
  selector: 'app-rag-document-add',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './add.html',
  standalone: true
})
export class RagDocumentAdd implements OnInit {

  i18nKeys = I18N_KEYS;

  acceptFilesTypes = '.pdf,.docx,.txt,.md,.pptx,.csv,.xlsx,.json,.xml';

  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<RagDocumentAdd>);
  private translate = inject(TranslateService);
  private snackBar = inject(MatSnackBar);
  private adminClient = inject(AdminClient);
  private destroyRef = inject(DestroyRef);

  form!: FormGroup;
  isLoading = signal(true);
  availableCollections = signal<RagCollectionItemDto[]>([]);
  selectedFile: File | null = null;
  uploadProgress = signal(0);
  isUploading = signal(false);

  constructor() {
    this.buildForm();
  }

  ngOnInit(): void {
    this.loadCollections();
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
      sourceUrl: [null, [Validators.maxLength(500)]],
      filePath: [null, []]  // 预留文件路径字段
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
  get filePath() { return this.form.get('filePath') as FormControl; }

  private loadCollections(): void {
    this.isLoading.set(true);
    this.adminClient.ragCollection.list({ pageIndex: 1, pageSize: 100 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.availableCollections.set(res.data || []);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
  }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
      this.fileName.setValue(file.name);
    }
  }

  uploadFile() {
    if (!this.selectedFile) {
      this.snackBar.open('Please select a file', '', { duration: 2000 });
      return;
    }

    this.isUploading.set(true);
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('folder', 'document');
    
    this.adminClient.fileUpload.uploadFile(formData).subscribe({
      next: (result: any) => {
        this.filePath.setValue(result.filePath);
        this.snackBar.open('File uploaded successfully', '', { duration: 2000 });
        this.isUploading.set(false);
      },
      error: (error: any) => {
        this.snackBar.open('File upload failed', '', { duration: 2000 });
        this.isUploading.set(false);
      }
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.adminClient.ragDocument.add(this.form.value as RagDocumentAddDto).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => this.snackBar.open('Save failed', '', { duration: 2000 })
    });
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
