/**
 * 应用定义UpdateDto
 */
export interface ApplicationUpdateDto {
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** accessKey */
  accessKey?: string | null;
  /** secretKey */
  secretKey?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
