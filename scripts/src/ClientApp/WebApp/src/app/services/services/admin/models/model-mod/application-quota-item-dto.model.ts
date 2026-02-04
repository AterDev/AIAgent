import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 应用配额 ItemDto
 */
export interface ApplicationQuotaItemDto {
  /** id */
  id: string;
  /** applicationId */
  applicationId: string;
  /** periodType */
  periodType: QuotaPeriodType;
  /** maxRequests */
  maxRequests: number;
  /** maxTokens */
  maxTokens: number;
  /** windowSeconds */
  windowSeconds: number;
  /** isEnabled */
  isEnabled: boolean;
}
