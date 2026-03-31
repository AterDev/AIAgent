/**
 * 应用定义DetailDto
 */
export interface ApplicationDetailDto {
  /** name */
  name: string;
  /** description */
  description: string;
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
