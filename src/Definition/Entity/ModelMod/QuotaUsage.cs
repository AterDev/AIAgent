namespace Entity.ModelMod;

/// <summary>
/// 配额使用统计（用于快速查询当前用量）
/// </summary>
[Index(nameof(ApplicationId), nameof(PeriodType), nameof(WindowStart))]
public class QuotaUsage : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    public QuotaPeriodType PeriodType { get; set; }

    /// <summary>
    /// 窗口开始时间
    /// </summary>
    public DateTime WindowStart { get; set; }

    /// <summary>
    /// 窗口结束时间
    /// </summary>
    public DateTime WindowEnd { get; set; }

    /// <summary>
    /// 请求数
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// 消耗的 Token 数
    /// </summary>
    public long TokensUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
