import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具 AddDto
 */
export interface McpToolAddDto {
  /** name */
  name: string;
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
