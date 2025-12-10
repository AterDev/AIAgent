namespace Entity.AIAgentMod;
/// <summary>
/// 用户Token用量信息
/// </summary>

[Index(nameof(UserId), nameof(CreatedTime))]
public class TokenUsageRecord : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public required string ModelId { get; set; }

    /// <summary>
    /// 成本
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int UsedTokens { get; set; }

}