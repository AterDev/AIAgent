/**
 * 系统配置ItemDto
 */
export interface SystemConfigItemDto {
  /** key */
  key: string;
  /** valid */
  valid: boolean;
  /** 是否属于系统配置 */
  isSystem: boolean;
  /** 组 */
  groupName: string;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
}
