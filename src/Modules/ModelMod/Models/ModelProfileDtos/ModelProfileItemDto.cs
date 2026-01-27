using Entity.ModelMod;
namespace ModelMod.Models.ModelProfileDtos;

/// <summary>
/// 模型配置 ItemDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProfile"/>
public class ModelProfileItemDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public bool IsEnabled { get; set; }
}
