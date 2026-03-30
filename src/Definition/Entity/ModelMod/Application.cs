namespace Entity.ModelMod;

/// <summary>
/// 应用定义
/// </summary>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(ClientId), IsUnique = true)]
public class Application : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public required string ClientId { get; set; }

    [MaxLength(200)]
    public required string SecretHash { get; set; }

    [MaxLength(100)]
    public required string SecretSalt { get; set; }

    public DateTimeOffset SecretUpdatedTime { get; set; } = DateTimeOffset.UtcNow;

    public bool IsEnabled { get; set; } = true;
}
