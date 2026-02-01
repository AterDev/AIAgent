/**
 * 文档解析状态
 */
export enum DocumentParsingStatus {
  /** Pending */
  Pending = 0,
  /** Parsing */
  Parsing = 1,
  /** Success */
  Success = 2,
  /** Failed */
  Failed = 3,
  /** Cancelled */
  Cancelled = 4,
}
