using Entity.ModelMod;
namespace ModelMod.Models.ModelProviderDtos;

/// <summary>
/// 模型提供商 ItemDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProvider"/>
public class ModelProviderItemDto
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? BaseUrl { get; set; }

    public ModelProviderType ProviderType { get; set; }

    public bool IsEnabled { get; set; }
}
