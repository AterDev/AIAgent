import { ArticleCategory } from '../entity/article-category.model';

/**
 * 目录概要
 */
export interface ArticleCategoryDetailDto {
  /** 目录名称 */
  name: string;
  /** 层级 */
  level: number;
  /** 目录 */
  parent: ArticleCategory;
  /** parentId */
  parentId?: string | null;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
}
