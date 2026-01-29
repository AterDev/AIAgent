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
import { McpToolUpdateDto } from 'src/app/services/admin/models/mcp-mod/mcp-tool-update-dto.model';
import { McpToolDetailDto } from 'src/app/services/admin/models/mcp-mod/mcp-tool-detail-dto.model';

@Component({
  selector: 'app-mcp-tool-edit',
  imports: [CommonFormModules, MatCheckboxModule, MatProgressSpinnerModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class McpToolEdit implements OnInit {

  i18nKeys = I18N_KEYS;

  form!: FormGroup;
  id?: string;
  isLoading = signal(false);

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private dialogRef: MatDialogRef<McpToolEdit>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private translate: TranslateService
  ) {
    this.buildForm();
    this.id = data?.id;
  }

  ngOnInit() {
    if (this.id) {
      this.isLoading.set(true);
      this.adminClient.mcpTool.detail(this.id).subscribe({
        next: (res: McpToolDetailDto) => {
          this.form.patchValue(res);
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
      "toolType": [null, [Validators.required]],
      "version": [null, [Validators.maxLength(40)]],
      "isEnabled": [null, []],
      "schemaJson": [null, [Validators.maxLength(4000)]],
      "serverId": [null, []]
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get toolType() { return this.form.get('toolType') as FormControl; }
  get version() { return this.form.get('version') as FormControl; }
  get isEnabled() { return this.form.get('isEnabled') as FormControl; }
  get schemaJson() { return this.form.get('schemaJson') as FormControl; }
  get serverId() { return this.form.get('serverId') as FormControl; }

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
    this.adminClient.mcpTool.update(this.id, this.form.value as McpToolUpdateDto).subscribe(() => this.dialogRef.close(true));
  }

  close(result: boolean) { this.dialogRef.close(result); }
}
