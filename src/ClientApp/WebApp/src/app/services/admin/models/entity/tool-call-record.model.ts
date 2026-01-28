import { McpTool } from '../entity/mcp-tool.model';
import { ToolCallStatus } from '../entity/tool-call-status.model';

/**
 * MCP 工具调用记录
 */
export interface ToolCallRecord {
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
  /** toolId */
  toolId: string;
  /** MCP 工具定义 */
  tool: McpTool;
  /** applicationId */
  applicationId?: string | null;
  /** agentId */
  agentId?: string | null;
  /** inputJson */
  inputJson: string;
  /** outputJson */
  outputJson: string;
  /** durationMs */
  durationMs: number;
  /** status */
  status: ToolCallStatus;
  /** errorMessage */
  errorMessage?: string | null;
}
