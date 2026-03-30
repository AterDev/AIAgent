namespace ModelMod.Models.ApplicationDtos;

/// <summary>
/// 应用凭证返回（仅创建/重置时返回明文密钥）
/// </summary>
public class ApplicationCredentialResultDto
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(100)]
    public string ClientId { get; set; } = default!;

    [MaxLength(200)]
    public string ClientSecret { get; set; } = default!;

    public bool IsEnabled { get; set; }

    public DateTimeOffset SecretUpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}