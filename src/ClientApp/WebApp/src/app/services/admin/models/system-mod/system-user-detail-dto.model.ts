import { GenderType } from '../ater/gender-type.model';

/**
 * 系统用户概要
 */
export interface SystemUserDetailDto {
  /** 用户名 */
  userName: string;
  /** 真实姓名 */
  realName?: string | null;
  /** email */
  email?: string | null;
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
  /** 密码重试次数 */
  retryCount: number;
  /** 头像url */
  avatar?: string | null;
  /** sex */
  sex: GenderType;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
}
