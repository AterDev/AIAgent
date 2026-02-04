/**
 * 提示词DetailDto
 */
export interface AIPromptDetailDto {
  /** 提示词名称 */
  name: string;
  /** 提示词描述 */
  description?: string | null;
  /** 提示词内容 */
  content: string;
  /** 提示词分组 */
  groupName: string;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
}
