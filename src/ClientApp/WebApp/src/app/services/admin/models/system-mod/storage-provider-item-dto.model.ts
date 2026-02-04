/**
 * 存储服务商ItemDto
 */
export interface StorageProviderItemDto {
  /** 存储服务商名称 */
  name: string;
  /** 是否为云存储 */
  isCloud: boolean;
  /** 存储桶名称 */
  bucketName?: string | null;
  /** 存储区域 */
  region?: string | null;
  /** 是否启用 */
  isActive: boolean;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
}
