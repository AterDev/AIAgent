using Entity.AIAgentMod;
namespace AIAgentMod.Models.ConversationDtos;
/// <summary>
/// 对话实例FilterDto
/// </summary>
/// <see cref="Entity.AIAgentMod.Conversation"/>
public class ConversationFilterDto : FilterBase
{
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsPinned { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }
    
}
