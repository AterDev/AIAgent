import { Application } from '../entity/application.model';

/**
 * 应用 MCP 工具权限
 */
export interface ApplicationToolPermission {
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
  /** applicationId */
  applicationId: string;
  /** 应用定义 */
  application: Application;
  /** toolName */
  toolName: string;
  /** isEnabled */
  isEnabled: boolean;
}
