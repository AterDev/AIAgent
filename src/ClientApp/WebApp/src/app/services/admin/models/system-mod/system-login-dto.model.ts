/**
 * 登录
 */
export interface SystemLoginDto {
  /** email */
  email: string;
  /** password */
  password: string;
  /** 验证码 */
  verifyCode?: string | null;
}
