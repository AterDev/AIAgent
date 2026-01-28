import { Application } from '../entity/application.model';
import { QuotaPeriodType } from '../entity/quota-period-type.model';

/**
 * 应用配额与限流
 */
export interface ApplicationQuota {
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
  /** periodType */
  periodType: QuotaPeriodType;
  /** 最大请求次数 */
  maxRequests: number;
  /** 最大 Token 数量 */
  maxTokens: number;
  /** 窗口秒数（限流窗口） */
  windowSeconds: number;
  /** isEnabled */
  isEnabled: boolean;
}
