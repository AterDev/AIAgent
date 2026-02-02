/**
 * 模型调试响应
 */
export interface ModelDebugResponseDto {
  /**
   * 生成的内容
   */
  content: string;
  /**
   * 使用的模型
   */
  model: string;
  /**
   * 提示词Token数
   */
  promptTokens: number;
  /**
   * 生成Token数
   */
  completionTokens: number;
  /**
   * 总Token数
   */
  totalTokens: number;
  /**
   * 完成原因
   */
  finishReason: string;
  /**
   * 调用耗时(毫秒)
   */
  duration: number;
  /**
   * 错误信息(如果有)
   */
  errorMessage?: string;
}
