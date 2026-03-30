/**
 * 应用定义DetailDto
 */
export interface ApplicationDetailDto {
  /** name */
  name: string;
  /** description */
  description: string;
  /** clientId */
  clientId: string;
  /** hasSecret */
  hasSecret: boolean;
  /** secretUpdatedTime */
  secretUpdatedTime: Date;
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
