/**
 * 令牌信息
 */
export interface AccessTokenDto {
  /** accessToken */
  accessToken: string;
  /** refreshToken */
  refreshToken: string;
  /** 过期时间秒 */
  expiresIn: number;
  /** refreshExpiresIn */
  refreshExpiresIn: number;
}
