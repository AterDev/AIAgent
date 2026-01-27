namespace Entity.ModelMod;

/// <summary>
/// 模型提供商/渠道配置
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class ModelProvider : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public required string BaseUrl { get; set; }

    [MaxLength(2000)]
    public required string ApiKey { get; set; }

    public ModelProviderType ProviderType { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int RetryCount { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }
}
