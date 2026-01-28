/**
 * 模型配置 ItemDto
 */
export interface ModelProfileItemDto {
  /** id */
  id: string;
  /** providerId */
  providerId: string;
  /** name */
  name?: string | null;
  /** displayName */
  displayName?: string | null;
  /** isEnabled */
  isEnabled: boolean;
}
