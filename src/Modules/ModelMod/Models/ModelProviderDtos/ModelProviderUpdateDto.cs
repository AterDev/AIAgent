namespace ModelMod.Models.ModelProviderDtos;

/// <summary>
/// 模型提供商 UpdateDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProvider"/>
public class ModelProviderUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? BaseUrl { get; set; }

    [MaxLength(2000)]
    public string? ApiKey { get; set; }

    public ModelProviderType? ProviderType { get; set; }

    public int? TimeoutSeconds { get; set; }

    public int? RetryCount { get; set; }

    public bool? IsEnabled { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
