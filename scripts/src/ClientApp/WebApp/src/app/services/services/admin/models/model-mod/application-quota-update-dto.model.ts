import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 应用配额 UpdateDto
 */
export interface ApplicationQuotaUpdateDto {
  /** periodType */
  periodType?: QuotaPeriodType | null;
  /** maxRequests */
  maxRequests?: number | null;
  /** maxTokens */
  maxTokens?: number | null;
  /** windowSeconds */
  windowSeconds?: number | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
