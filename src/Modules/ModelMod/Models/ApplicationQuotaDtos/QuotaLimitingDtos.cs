namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 配额检查请求
/// </summary>
public class QuotaCheckDto
{
    public Guid ApplicationId { get; set; }

    [Range(1, 100000)]
    public int EstimatedTokens { get; set; }
}

public class QuotaCheckRequestDto
{
    public Guid ApplicationId { get; set; }

    [Range(1, 100000)]
    public int EstimatedTokens { get; set; }
}

/// <summary>
/// 配额消耗请求
/// </summary>
public class QuotaConsumeDto
{
    public Guid ApplicationId { get; set; }

    [Range(1, 100000)]
    public int ActualTokens { get; set; }
}

public class QuotaConsumeRequestDto
{
    public Guid ApplicationId { get; set; }

    [Range(1, 100000)]
    public int ActualTokens { get; set; }
}

/// <summary>
/// 配额重置请求
/// </summary>
public class QuotaResetRequestDto
{
    public Guid ApplicationId { get; set; }

    public QuotaPeriodType PeriodType { get; set; }
}

/// <summary>
/// 配额消耗结果
/// </summary>
public class QuotaConsumeResultDto
{
    public bool Success { get; set; }

    public long RemainingTokens { get; set; }

    public int RemainingRequests { get; set; }

    public DateTime WindowStart { get; set; }

    public DateTime WindowEnd { get; set; }

    /// <summary>
    /// 使用百分比 (0-100)
    /// </summary>
    public double UsagePercentage { get; set; }
}

/// <summary>
/// 配额使用情况
/// </summary>
public class QuotaUsageDto
{
    public Guid ApplicationId { get; set; }

    public QuotaPeriodType PeriodType { get; set; }

    public int MaxRequests { get; set; }

    public long MaxTokens { get; set; }

    public int CurrentRequests { get; set; }

    public long CurrentTokens { get; set; }

    public DateTime WindowStart { get; set; }

    public DateTime WindowEnd { get; set; }

    /// <summary>
    /// 使用百分比
    /// </summary>
    public double UsagePercentage => CurrentTokens > 0 ? (CurrentTokens * 100.0) / MaxTokens : 0;
}
