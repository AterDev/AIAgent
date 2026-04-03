namespace Entity.ModelMod;

/// <summary>
/// 应用配额与限流
/// </summary>
[Index(nameof(ApplicationId), nameof(PeriodType), IsUnique = true)]
public class ApplicationQuota : EntityBase
{
    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    public QuotaPeriodType PeriodType { get; set; } = QuotaPeriodType.Month;

    /// <summary>
    /// 最大请求次数
    /// </summary>
    public int MaxRequests { get; set; } = 10_000;

    /// <summary>
    /// 最大 Token 数量
    /// </summary>
    public long MaxTokens { get; set; } = 100_000_000;

    /// <summary>
    /// 窗口秒数（限流窗口）
    /// </summary>
    public int WindowSeconds { get; set; }

    public bool IsEnabled { get; set; } = true;
}
