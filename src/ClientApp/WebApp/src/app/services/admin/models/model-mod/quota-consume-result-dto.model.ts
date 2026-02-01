/**
 * 配额消耗结果
 */
export interface QuotaConsumeResultDto {
  /** success */
  success: boolean;
  /** remainingTokens */
  remainingTokens: number;
  /** remainingRequests */
  remainingRequests: number;
  /** windowStart */
  windowStart: Date;
  /** windowEnd */
  windowEnd: Date;
  /** 使用百分比 (0-100) */
  usagePercentage: number;
}
