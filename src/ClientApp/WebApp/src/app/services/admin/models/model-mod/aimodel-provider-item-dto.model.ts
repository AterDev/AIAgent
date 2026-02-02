/**
 * AI模型提供商ItemDto
 */
export interface AIModelProviderItemDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** logoUrl */
  logoUrl?: string | null;
  /** website */
  website?: string | null;
  /** apiKey */
  apiKey?: string | null;
  /** baseUrl */
  baseUrl?: string | null;
}
