import { SystemRole } from '../entity/system-role.model';
import { SystemLogs } from '../entity/system-logs.model';
import { SystemOrganization } from '../entity/system-organization.model';
import { Sex } from '../entity/sex.model';

/**
 * 系统用户
 */
export interface SystemUser {
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
  /** 用户名 */
  userName?: string | null;
  /** 真实姓名 */
  realName?: string | null;
  /** email */
  email: string;
  /** emailConfirmed */
  emailConfirmed: boolean;
  /** phoneNumber */
  phoneNumber?: string | null;
  /** phoneNumberConfirmed */
  phoneNumberConfirmed: boolean;
  /** twoFactorEnabled */
  twoFactorEnabled: boolean;
  /** lockoutEnd */
  lockoutEnd?: Date | null;
  /** lockoutEnabled */
  lockoutEnabled: boolean;
  /** accessFailedCount */
  accessFailedCount: number;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** 最后密码修改时间 */
  lastPwdEditTime: Date;
  /** 密码重试次数 */
  retryCount: number;
  /** 头像url */
  avatar?: string | null;
  /** systemRoles */
  systemRoles: SystemRole[];
  /** systemLogs */
  systemLogs: SystemLogs[];
  /** systemOrganizations */
  systemOrganizations: SystemOrganization[];
  /** 性别 */
  sex: Sex;
}
