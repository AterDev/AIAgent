import { McpToolType } from '../entity/mcp-tool-type.model';

/**
 * 工具定义（对外）
 */
export interface ToolDefinitionDto {
  /** name */
  name: string;
  /** description */
  description: string;
  /** schemaJson */
  schemaJson?: string | null;
  /** version */
  version: string;
  /** toolType */
  toolType: McpToolType;
}
