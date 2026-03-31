namespace Entity.ModelMod;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// 应用 ApiKey 认证索引
/// </summary>
[Index(nameof(ApplicationId))]
[Index(nameof(KeyFingerprint), IsUnique = true)]
public class ApiKeyAuthIndex : EntityBase
{
    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public required string ApplicationName { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(64)]
    public required string KeyFingerprint { get; set; }

    [MaxLength(200)]
    public required string KeyHash { get; set; }

    [MaxLength(100)]
    public required string KeySalt { get; set; }

    public DateTimeOffset KeyUpdatedTime { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset KeyExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(3);
}