using Entity.AIAgentMod;
namespace AIAgentMod.Models.ConversationDtos;
/// <summary>
/// 对话实例ItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.Conversation"/>
public class ConversationItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsPinned { get; set; }
    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTimeOffset? LastActiveTime { get; set; }
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
    public int? TotalTokens { get; set; }
    
}
