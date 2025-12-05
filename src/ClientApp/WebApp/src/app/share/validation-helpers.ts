import { TranslateService } from '@ngx-translate/core';
import {
  ValidationError,
  MinLengthValidationError,
  MaxLengthValidationError,
  MinValidationError,
  MaxValidationError
} from '@angular/forms/signals';

function mapValidationParams(error: ValidationError): Record<string, unknown> | undefined {
  switch (error.kind) {
    case 'minLength':
      return { requiredLength: (error as MinLengthValidationError).minLength };
    case 'maxLength':
      return { requiredLength: (error as MaxLengthValidationError).maxLength };
    case 'min':
      return { min: (error as MinValidationError).min };
    case 'max':
      return { max: (error as MaxValidationError).max };
    default:
      return undefined;
  }
}

export function translateValidationError(
  translate: TranslateService,
  error: ValidationError,
  namespace = 'validation'
): string {
  const key = `${namespace}.${error.kind.toLowerCase()}`;
  const params = mapValidationParams(error);
  return error.message ?? translate.instant(key, params);
}
