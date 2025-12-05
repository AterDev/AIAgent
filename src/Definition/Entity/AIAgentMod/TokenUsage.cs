namespace Entity.AIAgentMod;
/// <summary>
/// 用户Token用量信息
/// </summary>

public class TokenUsage : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 总Token数量
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 已用Token数量
    /// </summary>
    public int UsedTokens { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpirateTime { get; set; }
}