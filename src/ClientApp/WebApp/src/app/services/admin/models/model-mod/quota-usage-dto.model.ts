import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 配额使用情况
 */
export interface QuotaUsageDto {
  /** applicationId */
  applicationId: string;
  /** periodType */
  periodType: QuotaPeriodType;
  /** maxRequests */
  maxRequests: number;
  /** maxTokens */
  maxTokens: number;
  /** currentRequests */
  currentRequests: number;
  /** currentTokens */
  currentTokens: number;
  /** windowStart */
  windowStart: Date;
  /** windowEnd */
  windowEnd: Date;
  /** 使用百分比 */
  usagePercentage: number;
}
