using Entity.ModelMod;
namespace ModelMod.Models.ModelProfileDtos;

/// <summary>
/// 模型配置 DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.ModelProfile"/>
public class ModelProfileDetailDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

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

    public bool IsEnabled { get; set; }
}
