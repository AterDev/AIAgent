import { AuthType } from '../entity/auth-type.model';
import { TransportType } from '../entity/transport-type.model';

/**
 * McpServerFilterDto
 */
export interface MCPServerInfoFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** authType */
  authType?: AuthType | null;
  /** MCP Server 名称 */
  displayName?: string | null;
  /** 唯一标识 */
  identityName?: string | null;
  /** transportType */
  transportType?: TransportType | null;
}
