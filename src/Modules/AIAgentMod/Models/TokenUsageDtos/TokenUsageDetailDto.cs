using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageDtos;
/// <summary>
/// 用户Token用量信息DetailDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsage"/>
public class TokenUsageDetailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset? ExpirateTime { get; set; }
    public Guid TenantId { get; set; }
    /// <summary>
    /// 总Token数量
    /// </summary>
    public int? TotalTokens { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int? UsedTokens { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }
    
}
