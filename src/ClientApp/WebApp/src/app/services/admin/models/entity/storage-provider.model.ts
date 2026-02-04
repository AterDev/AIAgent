/**
 * 存储服务商
 */
export interface StorageProvider {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** 存储服务商名称 */
  name: string;
  /** 是否为云存储 */
  isCloud: boolean;
  /** 本地存储路径 */
  path?: string | null;
  /** 访问端点 */
  endpoint?: string | null;
  /** 访问密钥ID */
  accessKeyId?: string | null;
  /** 访问密钥密码 */
  accessKeySecret?: string | null;
  /** 存储桶名称 */
  bucketName?: string | null;
  /** 存储区域 */
  region?: string | null;
  /** 是否启用 */
  isActive: boolean;
}
