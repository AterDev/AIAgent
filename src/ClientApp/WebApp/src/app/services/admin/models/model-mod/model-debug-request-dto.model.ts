/**
 * 模型调试请求
 */
export interface ModelDebugRequestDto {
  /**
   * 模型ID
   */
  modelId: string;
  /**
   * 用户提示词
   */
  prompt: string;
  /**
   * 系统提示词
   */
  systemPrompt?: string;
  /**
   * 温度参数 (0-2)
   */
  temperature?: number;
  /**
   * 最大生成Token数
   */
  maxTokens?: number;
}
