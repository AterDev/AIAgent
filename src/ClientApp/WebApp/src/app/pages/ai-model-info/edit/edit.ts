import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { AIModelInfoUpdateDto } from 'src/app/services/admin/models/model-mod/aimodel-info-update-dto.model';
import { AIModelProviderItemDto } from 'src/app/services/admin/models/model-mod/aimodel-provider-item-dto.model';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-ai-model-info-edit',
  imports: [CommonFormModules, CommonModule, TranslateModule, MatCard, MatCardHeader, MatCardTitle, MatCardContent, MatCardActions],
  templateUrl: './edit.html',
  standalone: true
})
export class AIModelInfoEdit implements OnInit {
  i18nKeys = I18N_KEYS;
  form!: FormGroup;
  isLoading = signal(false);
  id: string = '';
  providers: AIModelProviderItemDto[] = [];

  constructor(
    private fb: FormBuilder,
    private adminClient: AdminClient,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      providerId: ['', Validators.required],
      contextLength: [4096, [Validators.required, Validators.min(0)]],
      inputPrice: [0, [Validators.required, Validators.min(0)]],
      outputPrice: [0, [Validators.required, Validators.min(0)]]
    });
    this.loadProviders();
    this.route.params.subscribe(params => {
      this.id = params['id'];
      this.loadData();
    });
  }

  get name() { return this.form.get('name') as FormControl; }
  get description() { return this.form.get('description') as FormControl; }
  get providerId() { return this.form.get('providerId') as FormControl; }
  get contextLength() { return this.form.get('contextLength') as FormControl; }
  get inputPrice() { return this.form.get('inputPrice') as FormControl; }
  get outputPrice() { return this.form.get('outputPrice') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }

  loadProviders(): void {
    this.adminClient.aIModelProvider.list({ pageIndex: 1, pageSize: 1000 }).subscribe((res: any) => {
      this.providers = res.data || [];
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.adminClient.aIModelInfo.detail(this.id).subscribe({
      next: (data: any) => {
        this.form.patchValue(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isLoading.set(true);
    this.adminClient.aIModelInfo.update(this.id, this.form.value as AIModelInfoUpdateDto).subscribe({
      next: () => {
        this.router.navigate(['/ai-model-info/index']);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onCancel(): void {
    this.router.navigate(['/ai-model-info/index']);
  }
}
