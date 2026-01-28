/**
 * 应用定义DetailDto
 */
export interface ApplicationDetailDto {
  /** name */
  name: string;
  /** description */
  description: string;
  /** accessKey */
  accessKey: string;
  /** secretKey */
  secretKey: string;
  /** isEnabled */
  isEnabled: boolean;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
