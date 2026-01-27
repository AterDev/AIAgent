namespace ModelMod.Models.ModelProviderDtos;

/// <summary>
/// 模型提供商 FilterDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProvider"/>
public class ModelProviderFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public ModelProviderType? ProviderType { get; set; }

    public bool? IsEnabled { get; set; }
}
