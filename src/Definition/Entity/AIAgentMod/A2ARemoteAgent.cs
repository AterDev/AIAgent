namespace Entity.AIAgentMod;

/// <summary>
/// Remote agent endpoint for Agent-to-Agent (A2A) protocol communication.
/// Represents an external agent that can be invoked via the Google A2A protocol.
/// </summary>
[Index(nameof(Name))]
[Index(nameof(AgentUrl), nameof(TenantId), IsUnique = true)]
public class A2ARemoteAgent : EntityBase
{
    /// <summary>
    /// 远程 Agent 名称
    /// </summary>
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// 远程 Agent 描述
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Agent URL (base URL, agent card at /.well-known/agent.json)
    /// </summary>
    [MaxLength(500)]
    public required string AgentUrl { get; set; }

    /// <summary>
    /// 认证类型
    /// </summary>
    public AuthType AuthType { get; set; }

    /// <summary>
    /// 认证值（Bearer token / API Key）
    /// </summary>
    public string? AuthValue { get; set; }

    /// <summary>
    /// 远程 Agent 支持的技能列表（从 Agent Card 获取，缓存用）
    /// </summary>
    public List<string>? Skills { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
