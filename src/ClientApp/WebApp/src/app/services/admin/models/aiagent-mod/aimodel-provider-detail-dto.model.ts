/**
 * AI模型提供商DetailDto
 */
export interface AIModelProviderDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** 说明 */
  description?: string | null;
  /** logoUrl */
  logoUrl?: string | null;
  /** 提供商名称 */
  name?: string | null;
  /** tenantId */
  tenantId: string;
  /** updatedTime */
  updatedTime: Date;
  /** 官网地址 */
  website?: string | null;
}
