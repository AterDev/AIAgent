using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageDtos;
/// <summary>
/// 用户Token用量信息ItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsage"/>
public class TokenUsageItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset? ExpirateTime { get; set; }
    /// <summary>
    /// 总Token数量
    /// </summary>
    public int? TotalTokens { get; set; }
    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int? UsedTokens { get; set; }
    
}
