namespace Entity.AIAgentMod;

/// <summary>
/// 聊天消息
/// </summary>

[Index(nameof(UserId), nameof(ConversationId), nameof(CreatedTime))]
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
    public ChatMessageRole Role { get; set; } = ChatMessageRole.User;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 内容类型（文本、图片、文件等）
    /// </summary>
    [MaxLength(32)]
    public ChatMessageType ContentType { get; set; } = ChatMessageType.Text;

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

public enum ChatMessageRole
{
    User,
    AI,
    System,
    Tool
}

public enum ChatMessageType
{
    Text,
    Image,
    File
}
