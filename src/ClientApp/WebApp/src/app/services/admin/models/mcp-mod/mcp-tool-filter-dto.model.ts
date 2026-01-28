import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * MCP 工具 FilterDto
 */
export interface McpToolFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** toolType */
  toolType?: McpToolType | null;
  /** isEnabled */
  isEnabled?: boolean | null;
}
