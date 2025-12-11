using Entity.AIAgentMod;
namespace AIAgentMod.Models.TokenUsageRecordDtos;
/// <summary>
/// 用户Token用量信息AddDto
/// </summary>
/// <see cref="Entity.AIAgentMod.TokenUsageRecord"/>
public class TokenUsageRecordAddDto
{
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ModelId { get; set; } = default!;
    /// <summary>
    /// 成本
    /// </summary>
    public decimal Price { get; set; }
    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int UsedTokens { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
}
