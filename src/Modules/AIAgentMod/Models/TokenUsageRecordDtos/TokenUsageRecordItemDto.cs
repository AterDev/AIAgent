using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageRecordDtos;
/// <summary>
/// 用户Token用量信息ItemDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsageRecord"/>
public class TokenUsageRecordItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 成本
    /// </summary>
    public decimal? Price { get; set; }
    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int? UsedTokens { get; set; }
    
}
