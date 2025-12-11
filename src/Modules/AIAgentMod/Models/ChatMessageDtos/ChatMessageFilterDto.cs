using Entity.AIAgentMod;
namespace AIAgentMod.Models.ChatMessageDtos;
/// <summary>
/// 聊天消息FilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.ChatMessage"/>
public class ChatMessageFilterDto : FilterBase
{
    /// <summary>
    /// 内容类型（文本、图片、文件等）
    /// </summary>
    [MaxLength(32)]
    public ChatMessageType? ContentType { get; set; }
    /// <summary>
    /// 角色（用户、AI等）
    /// </summary>
    [MaxLength(32)]
    public ChatMessageRole? Role { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }
    public Guid? ConversationId { get; set; }
    
}
