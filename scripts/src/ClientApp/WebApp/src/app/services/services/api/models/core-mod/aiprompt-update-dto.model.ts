/**
 * 提示词UpdateDto
 */
export interface AIPromptUpdateDto {
  /** 提示词名称 */
  name?: string | null;
  /** 提示词描述 */
  description?: string | null;
  /** 提示词内容 */
  content?: string | null;
  /** 提示词分组 */
  groupName?: string | null;
}
