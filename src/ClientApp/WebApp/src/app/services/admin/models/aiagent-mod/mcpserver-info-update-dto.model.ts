import { AuthType } from '../entity/auth-type.model';
import { TransportType } from '../entity/transport-type.model';

/**
 * McpServerUpdateDto
 */
export interface MCPServerInfoUpdateDto {
  /** StdIO 模式下的启动参数 */
  arguments?: string[] | null;
  /** authType */
  authType?: AuthType | null;
  /** 认证值（API Key 或 Token） */
  authValue?: string | null;
  /** 描述信息 */
  description?: string | null;
  /** MCP Server 名称 */
  displayName?: string | null;
  /** HTTP / SSE / WebSocket 模式下的连接地址 */
  endpoint?: string | null;
  /** StdIO 模式下的可执行文件路径 */
  executablePath?: string | null;
  /** 唯一标识 */
  identityName?: string | null;
  /** transportType */
  transportType?: TransportType | null;
}
