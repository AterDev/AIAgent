/**
 * 文档解析/向量化输入
 */
export interface RagDocumentIngestDto {
  /** 直接提供文本内容（可选） */
  contentText?: string | null;
}
