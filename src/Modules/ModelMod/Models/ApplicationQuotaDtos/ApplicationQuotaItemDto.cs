namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 应用配额 ItemDto
/// </summary>
/// <see cref="ApplicationQuota"/>
public class ApplicationQuotaItemDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public QuotaPeriodType PeriodType { get; set; }
    public int MaxRequests { get; set; }
    public long MaxTokens { get; set; }
    public int WindowSeconds { get; set; }
    public bool IsEnabled { get; set; }
}
