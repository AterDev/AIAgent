import { Component, input, output, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface FilterConfig {
  field: string;
  label: string;
  type: 'text' | 'select' | 'date' | 'daterange' | 'number';
  placeholder?: string;
  options?: { label: string; value: any }[];
  defaultValue?: any;
}

@Component({
  selector: 'app-search-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatTooltipModule
  ],
  template: `
    <div class="search-panel">
      <form [formGroup]="searchForm" class="search-form">
        <!-- Quick Search -->
        <mat-form-field appearance="outline" class="search-input">
          <mat-label>Search</mat-label>
          <input matInput type="text" placeholder="Enter keyword..." formControlName="keyword">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>

        <!-- Actions -->
        <div class="search-actions">
          <button mat-raised-button color="primary" type="button" (click)="onSearch()">
            <mat-icon>search</mat-icon>
            Search
          </button>
          
          <button mat-stroked-button type="button" (click)="toggleAdvanced()">
            <mat-icon>{{ showAdvanced() ? 'expand_less' : 'expand_more' }}</mat-icon>
            {{ showAdvanced() ? 'Hide' : 'Show' }} Filters
          </button>

          <button mat-icon-button type="button" (click)="onClear()" matTooltip="Clear filters">
            <mat-icon>clear</mat-icon>
          </button>
        </div>
      </form>

      <!-- Advanced Filters -->
      @if (showAdvanced()) {
        <mat-expansion-panel [expanded]="true" class="filter-panel">
          <mat-expansion-panel-header>
            <mat-panel-title>Advanced Filters</mat-panel-title>
          </mat-expansion-panel-header>
          
          <div class="advanced-filters">
            @for (filter of filterConfigs(); track filter.field) {
              <mat-form-field appearance="outline">
                <mat-label>{{ filter.label }}</mat-label>
                
                @if (filter.type === 'text' || filter.type === 'number') {
                  <input 
                    matInput 
                    [type]="filter.type"
                    [formControlName]="filter.field"
                    [placeholder]="filter.placeholder || ''"
                  />
                }
                
                @if (filter.type === 'select') {
                  <mat-select [formControlName]="filter.field">
                    <mat-option value="">-- All --</mat-option>
                    @for (opt of filter.options; track opt.value) {
                      <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
                    }
                  </mat-select>
                }
                
                @if (filter.type === 'date') {
                  <input 
                    matInput 
                    type="date"
                    [formControlName]="filter.field"
                  />
                }
              </mat-form-field>
            }
          </div>
        </mat-expansion-panel>
      }
    </div>
  `,
  styles: [`
    .search-panel {
      margin-bottom: 1rem;
    }

    .search-form {
      display: flex;
      gap: 1rem;
      align-items: center;
      padding: 1rem;
      background: #fafafa;
      border-radius: 4px;
    }

    .search-input {
      flex: 1;
      min-width: 300px;
    }

    .search-actions {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .filter-panel {
      margin-top: 1rem;
    }

    .advanced-filters {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1rem;
      padding: 1rem;
    }
  `]
})
export class SearchPanelComponent implements OnInit {
  readonly filterConfigs = input<FilterConfig[]>([]);
  readonly searchEvent = output<Record<string, any>>();
  readonly clearEvent = output<void>();

  readonly showAdvanced = signal(false);
  
  private fb = inject(FormBuilder);
  searchForm!: FormGroup;

  ngOnInit() {
    this.buildForm();
  }

  private buildForm() {
    const controls: any = {
      keyword: ['']
    };

    this.filterConfigs().forEach(config => {
      controls[config.field] = [config.defaultValue || ''];
    });

    this.searchForm = this.fb.group(controls);
  }

  toggleAdvanced() {
    this.showAdvanced.set(!this.showAdvanced());
  }

  onSearch() {
    const values = this.searchForm.value;
    const filters: Record<string, any> = {};
    
    Object.keys(values).forEach(key => {
      if (values[key] !== null && values[key] !== undefined && values[key] !== '') {
        filters[key] = values[key];
      }
    });
    
    this.searchEvent.emit(filters);
  }

  onClear() {
    this.searchForm.reset();
    this.clearEvent.emit();
  }
}
