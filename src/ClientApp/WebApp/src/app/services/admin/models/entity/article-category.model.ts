import { Article } from '../entity/article.model';

/**
 * 目录
 */
export interface ArticleCategory {
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
  /** 目录名称 */
  name: string;
  /** 层级 */
  level: number;
  /** 子目录 */
  children: ArticleCategory[];
  /** 目录 */
  parent: ArticleCategory;
  /** parentId */
  parentId?: string | null;
  /** blogs */
  blogs: Article[];
  /** userId */
  userId: string;
}
