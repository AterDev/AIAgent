export interface PageList<T1> {
  /** count */
  count: number;
  /** data */
  data: T1[];
  /** pageIndex */
  pageIndex: number;
}
