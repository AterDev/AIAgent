import { UserActionType } from '../ater/user-action-type.model';

/**
 * 系统日志查询筛选
 */
export interface SystemLogsFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** 操作人名称 */
  actionUserName?: string | null;
  /** 操作对象名称 */
  targetName?: string | null;
  /** actionType */
  actionType?: UserActionType | null;
  /** 开始时间 */
  startDate?: string | null;
  /** 结束时间 */
  endDate?: string | null;
}
