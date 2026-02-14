namespace ModelMod.Models.ModelInvocationDtos;

/// <summary>
/// 调用记录 FilterDto
/// </summary>
/// <see cref="ModelInvocation"/>
public class ModelInvocationFilterDto : FilterBase
{
    public Guid? ApplicationId { get; set; }
    public Guid? AIModelInfoId { get; set; }

    [MaxLength(100)]
    public string? Scene { get; set; }

    public InvocationStatus? Status { get; set; }
}
