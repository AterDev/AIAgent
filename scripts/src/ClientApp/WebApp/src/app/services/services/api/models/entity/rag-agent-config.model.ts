import { AIModelInfo } from '../entity/aimodel-info.model';
import { AIPrompt } from '../entity/aiprompt.model';

/**
 * RAG 模型配置
 */
export interface RagAgentConfig {
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
  /** 配置项名称 */
  key: string;
  /** 配置项值 */
  value: string;
  /** 关联的 AI 模型 ID */
  aiModelInfoId?: string | null;
  /** 模型信息（包含能力和定价） */
  aiModelInfo: AIModelInfo;
  /** 配置项描述 */
  description?: string | null;
  /** 关联的提示词 ID */
  aiPromptId?: string | null;
  /** 提示词 */
  aiPrompt: AIPrompt;
}
