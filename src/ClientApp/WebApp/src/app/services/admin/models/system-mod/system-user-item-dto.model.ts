import { GenderType } from '../ater/gender-type.model';

/**
 * 系统用户列表元素
 */
export interface SystemUserItemDto {
  /** 用户名 */
  userName: string;
  /** 真实姓名 */
  realName?: string | null;
  /** email */
  email?: string | null;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** sex */
  sex: GenderType;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
}
