/**
 * AI模型提供商AddDto
 */
export interface AIModelProviderAddDto {
  /** 说明 */
  description?: string | null;
  /** logoUrl */
  logoUrl?: string | null;
  /** 提供商名称 */
  name: string;
  /** 官网地址 */
  website?: string | null;
}
