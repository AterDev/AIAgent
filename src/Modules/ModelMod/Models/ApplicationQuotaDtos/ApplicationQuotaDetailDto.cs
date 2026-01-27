namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 应用配额 DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationQuota"/>
public class ApplicationQuotaDetailDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public QuotaPeriodType PeriodType { get; set; }
    public int MaxRequests { get; set; }
    public int MaxTokens { get; set; }
    public int WindowSeconds { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }
}
