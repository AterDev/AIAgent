import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具 DetailDto
 */
export interface McpToolDetailDto {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** tenantId */
  tenantId: string;
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** toolType */
  toolType: McpToolType;
  /** version */
  version?: string | null;
  /** isEnabled */
  isEnabled: boolean;
  /** schemaJson */
  schemaJson?: string | null;
  /** serverId */
  serverId?: string | null;
}
