/**
 * 提示词
 */
export interface AIPrompt {
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
  /** 提示词名称 */
  name: string;
  /** 提示词描述 */
  description?: string | null;
  /** 提示词内容 */
  content: string;
  /** 提示词分组 */
  groupName: string;
}
