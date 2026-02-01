import { DocumentParsingStatus } from '../entity/document-parsing-status.model';

export interface DocumentParsingResultDto {
  /** id */
  id: string;
  /** ragDocumentId */
  ragDocumentId: string;
  /** 文档解析状态 */
  parsingStatus: DocumentParsingStatus;
  /** wordCount */
  wordCount: number;
  /** pageCount */
  pageCount?: number | null;
  /** durationMs */
  durationMs?: number | null;
  /** completedTime */
  completedTime?: Date | null;
  /** createdAt */
  createdAt: Date;
}
