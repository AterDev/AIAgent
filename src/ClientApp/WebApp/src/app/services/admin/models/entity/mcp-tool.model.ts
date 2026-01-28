import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具定义
 */
export interface McpTool {
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
  /** name */
  name: string;
  /** description */
  description: string;
  /** toolType */
  toolType: McpToolType;
  /** version */
  version: string;
  /** isEnabled */
  isEnabled: boolean;
  /** schemaJson */
  schemaJson?: string | null;
  /** serverId */
  serverId?: string | null;
}
