namespace ModelMod.Models.ModelProviderDtos;

/// <summary>
/// 模型提供商 AddDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProvider"/>
public class ModelProviderAddDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string BaseUrl { get; set; } = default!;

    [MaxLength(2000)]
    public string ApiKey { get; set; } = default!;

    public ModelProviderType ProviderType { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int RetryCount { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }
}
