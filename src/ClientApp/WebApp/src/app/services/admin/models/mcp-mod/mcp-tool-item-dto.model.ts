import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具 ItemDto
 */
export interface McpToolItemDto {
  /** id */
  id: string;
  /** name */
  name?: string | null;
  /** toolType */
  toolType: McpToolType;
  /** isEnabled */
  isEnabled: boolean;
}
