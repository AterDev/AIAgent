/**
 * 目录列表元素
 */
export interface ArticleCategoryItemDto {
  /** 目录名称 */
  name: string;
  /** 层级 */
  level: number;
  /** parentId */
  parentId?: string | null;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
}
