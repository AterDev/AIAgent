import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具 UpdateDto
 */
export interface McpToolUpdateDto {
  /** name */
  name?: string | null;
  /** description */
  description?: string | null;
  /** toolType */
  toolType?: McpToolType | null;
  /** version */
  version?: string | null;
  /** isEnabled */
  isEnabled?: boolean | null;
  /** schemaJson */
  schemaJson?: string | null;
  /** serverId */
  serverId?: string | null;
}
