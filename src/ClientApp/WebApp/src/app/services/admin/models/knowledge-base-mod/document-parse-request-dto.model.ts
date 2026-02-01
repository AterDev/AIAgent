/**
 * 文档解析请求
 */
export interface DocumentParseRequestDto {
  /** 文件路径（上传后返回） */
  filePath: string;
  /** 文件名 */
  fileName: string;
}
