namespace Entity.AIAgentMod;

/// <summary>
/// AI Agent 定义（基于 Microsoft Agent Framework 1.1 的 ChatClientAgent）
/// </summary>

[Index(nameof(Name), IsUnique = true)]
public class AIAgent : EntityBase
{
    /// <summary>
    /// Agent 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Agent 描述信息
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Agent 所使用的大模型名称（例如 "gpt-4.1", "deepseek-chat"）。
    /// 留空时使用 <see cref="ProviderId"/> + 默认模型。
    /// </summary>
    [MaxLength(200)]
    public required string ModelId { get; set; }

    /// <summary>
    /// 可选：绑定的模型提供商 Id（为空时根据 ModelId 自动解析）
    /// </summary>
    public Guid? ProviderId { get; set; }

    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    [MaxLength(8000)]
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Agent 可用的工具名称列表（MCP / 内置）
    /// </summary>
    public List<string> Tools { get; set; } = [];

    /// <summary>
    /// Agent 关联的 Skill 名称列表（由 AIFunctionFactory 暴露的业务函数）
    /// </summary>
    public List<string> Skills { get; set; } = [];

    /// <summary>
    /// 可 Handoff 的目标 Agent 名称列表（供工作流/对话编排使用）
    /// </summary>
    public List<string> HandoffTargets { get; set; } = [];

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 能力标志
    /// </summary>
    public AgentCapabilities Capabilities { get; set; } = AgentCapabilities.Tools | AgentCapabilities.Streaming;

    /// <summary>
    /// 记忆模式
    /// </summary>
    public AgentMemoryMode MemoryMode { get; set; } = AgentMemoryMode.Window;

    /// <summary>
    /// 上下文窗口（历史消息保留条数，对 Window/Summary 模式有效）
    /// </summary>
    public int ContextWindow { get; set; } = 20;

    /// <summary>
    /// 结构化输出的 JSON Schema（可选）
    /// </summary>
    [MaxLength(4000)]
    public string? ResponseSchemaJson { get; set; }

    /// <summary>
    /// 采样温度
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// TopP
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// 最大输出 token 数
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// 频率惩罚
    /// </summary>
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// 存在惩罚
    /// </summary>
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Agent 图标 URL
    /// </summary>
    [MaxLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 偏好输出语言（zh-CN、en-US 等）
    /// </summary>
    [MaxLength(20)]
    public string? OutputLanguage { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 是否公共（跨租户可用）
    /// </summary>
    public bool IsPublic { get; set; }
}
