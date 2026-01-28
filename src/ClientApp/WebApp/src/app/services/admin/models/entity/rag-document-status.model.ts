export enum RagDocumentStatus {
  /** Pending */
  Pending = 0,
  /** Parsing */
  Parsing = 1,
  /** Vectorizing */
  Vectorizing = 2,
  /** Completed */
  Completed = 3,
  /** Failed */
  Failed = 4,
}
