namespace Entity.AIAgentMod;

/// <summary>
/// McpServer
/// </summary>

[Index(nameof(Name))]
public class MCPServerInfo : EntityBase
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public required string IdentityName { get; set; }

    /// <summary>
    /// MCP Server 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 传输类型（http, stdio, sse, websocket）
    /// </summary>
    public TransportType TransportType { get; set; }

    /// <summary>
    /// HTTP / SSE / WebSocket 模式下的连接地址
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// StdIO 模式下的可执行文件路径
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// StdIO 模式下的启动参数
    /// </summary>
    public List<string>? Arguments { get; set; }

    /// <summary>
    /// 认证类型（None, ApiKey, OAuth, Token）
    /// </summary>
    public AuthType AuthType { get; set; }

    /// <summary>
    /// 认证值（API Key 或 Token）
    /// </summary>
    public string? AuthValue { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Description { get; set; }
}
public enum TransportType
{
    Http,
    Stdio,
    Sse,
    Websocket
}
public enum AuthType
{
    None,
    ApiKey,
    OAuth,
    Token
}
