namespace Entity.ModelMod;

/// <summary>
/// 模型元数据与能力
/// </summary>
[Index(nameof(ProviderId), nameof(Name), IsUnique = true)]
public class ModelProfile : EntityBase
{
    public Guid ProviderId { get; set; }

    [ForeignKey(nameof(ProviderId))]
    public ModelProvider? Provider { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int MaxContextTokens { get; set; }

    public bool SupportsChat { get; set; }

    public bool SupportsEmbedding { get; set; }

    public bool SupportsTools { get; set; }

    public bool SupportsVision { get; set; }

    public bool SupportsResponsesApi { get; set; }

    public bool IsEnabled { get; set; } = true;
}
