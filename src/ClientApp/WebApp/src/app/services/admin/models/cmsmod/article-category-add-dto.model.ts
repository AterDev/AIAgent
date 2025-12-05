/**
 * 目录添加时请求结构
 */
export interface ArticleCategoryAddDto {
  /** 目录名称 */
  name: string;
  /** parentId */
  parentId?: string | null;
}
