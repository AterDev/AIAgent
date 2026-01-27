using Entity.ModelMod;
namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 应用配额 AddDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationQuota"/>
public class ApplicationQuotaAddDto
{
    public Guid ApplicationId { get; set; }

    public QuotaPeriodType PeriodType { get; set; }

    public int MaxRequests { get; set; }

    public int MaxTokens { get; set; }

    public int WindowSeconds { get; set; }

    public bool IsEnabled { get; set; } = true;
}
