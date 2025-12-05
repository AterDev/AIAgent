namespace Entity.AIAgentMod;

/// <summary>
/// 对话实例
/// </summary>

[Index(nameof(UserId), nameof(Name))]
public class Conversation : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 对话名称
    /// </summary>
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 对话描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 使用的AI模型
    /// </summary>
    [MaxLength(128)]
    public string? ModelName { get; set; }

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// 总令牌数量
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTimeOffset LastActiveTime { get; set; }

    /// <summary>
    /// 对话中的消息列表
    /// </summary>
    public virtual ICollection<ChatMessage> Messages { get; set; } = [];
}