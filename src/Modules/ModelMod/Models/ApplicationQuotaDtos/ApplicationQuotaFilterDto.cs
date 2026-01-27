using Entity.ModelMod;
namespace ModelMod.Models.ApplicationQuotaDtos;

/// <summary>
/// 应用配额 FilterDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationQuota"/>
public class ApplicationQuotaFilterDto : FilterBase
{
    public Guid? ApplicationId { get; set; }
    public QuotaPeriodType? PeriodType { get; set; }
    public bool? IsEnabled { get; set; }
}
