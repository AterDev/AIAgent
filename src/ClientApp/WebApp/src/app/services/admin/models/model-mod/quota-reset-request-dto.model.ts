import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 配额重置请求
 */
export interface QuotaResetRequestDto {
  /** applicationId */
  applicationId: string;
  /** periodType */
  periodType: QuotaPeriodType;
}
