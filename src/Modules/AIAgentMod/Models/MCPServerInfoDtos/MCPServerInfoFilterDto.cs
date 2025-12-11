using Entity.AIAgentMod;
namespace AIAgentMod.Models.MCPServerInfoDtos;
/// <summary>
/// McpServerFilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.MCPServerInfo"/>
public class MCPServerInfoFilterDto : FilterBase
{
    /// <summary>
    /// 认证类型（None, ApiKey, OAuth, Token）
    /// </summary>
    public AuthType? AuthType { get; set; }
    /// <summary>
    /// MCP Server 名称
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// 唯一标识
    /// </summary>
    public string? IdentityName { get; set; }
    /// <summary>
    /// 传输类型（http, stdio, sse, websocket）
    /// </summary>
    public TransportType? TransportType { get; set; }
    
}
