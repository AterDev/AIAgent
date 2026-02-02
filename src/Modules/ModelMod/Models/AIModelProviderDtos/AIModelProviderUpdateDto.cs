namespace ModelMod.Models.AIModelProviderDtos;

/// <summary>
/// AI模型提供商UpdateDto
/// </summary>
public class AIModelProviderUpdateDto
{
    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 提供商名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// 官网地址
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }

    /// <summary>
    /// API密钥
    /// </summary>
    [MaxLength(200)]
    public string? ApiKey { get; set; }

    /// <summary>
    /// API基础URL
    /// </summary>
    [MaxLength(200)]
    public string? BaseUrl { get; set; }
}
