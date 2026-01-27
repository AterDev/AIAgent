using Entity.ModelMod;
namespace ModelMod.Models.ModelProfileDtos;

/// <summary>
/// 模型配置 FilterDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProfile"/>
public class ModelProfileFilterDto : FilterBase
{
    public Guid? ProviderId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    public bool? IsEnabled { get; set; }
}
