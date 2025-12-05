namespace Entity.AIAgentMod;

/// <summary>
/// 聊天消息
/// </summary>

[Index(nameof(UserId), nameof(Role))]
public class ChatMessage : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 对话ID
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 关联的对话
    /// </summary>
    [ForeignKey(nameof(ConversationId))]
    public Conversation? Conversation { get; set; }

    /// <summary>
    /// 角色（用户、AI等）
    /// </summary>
    [MaxLength(32)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 内容类型（文本、图片、文件等）
    /// </summary>
    [MaxLength(32)]
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// 令牌数量
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [MaxLength(128)]
    public string? ModelName { get; set; }

}
