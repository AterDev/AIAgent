import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 应用配额 FilterDto
 */
export interface ApplicationQuotaFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** applicationId */
  applicationId?: string | null;
  /** periodType */
  periodType?: QuotaPeriodType | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
