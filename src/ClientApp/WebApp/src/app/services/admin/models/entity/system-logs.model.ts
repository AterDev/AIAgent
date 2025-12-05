import { UserActionType } from '../ater/user-action-type.model';
import { SystemUser } from '../entity/system-user.model';

/**
 * 系统日志
 */
export interface SystemLogs {
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
  /** 操作人名称 */
  actionUserName: string;
  /** 操作对象名称 */
  targetName?: string | null;
  /** 操作路由 */
  route: string;
  /** actionType */
  actionType: UserActionType;
  /** 描述 */
  description?: string | null;
  /** 系统用户 */
  systemUser: SystemUser;
  /** systemUserId */
  systemUserId: string;
}
