using Entity.AIAgentMod;
namespace AIAgentMod.Models.ChatMessageDtos;
/// <summary>
/// 聊天消息AddDto
/// </summary>
/// <see cref="Entity.AIAgentMod.ChatMessage"/>
public class ChatMessageAddDto
{
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
    /// 模型名称
    /// </summary>
    [MaxLength(128)]
    public string? ModelName { get; set; }
    /// <summary>
    /// 角色（用户、AI等）
    /// </summary>
    [MaxLength(32)]
    public ChatMessageRole Role { get; set; } = ChatMessageRole.User;
    /// <summary>
    /// 令牌数量
    /// </summary>
    public int? TokenCount { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; } = default!;
    
}
