namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 应用配额 UpdateDto
/// </summary>
/// <see cref="ApplicationQuota"/>
public class ApplicationQuotaUpdateDto
{
    public QuotaPeriodType? PeriodType { get; set; }

    public int? MaxRequests { get; set; }

    public long? MaxTokens { get; set; }

    public int? WindowSeconds { get; set; }

    public bool? IsEnabled { get; set; }
}
