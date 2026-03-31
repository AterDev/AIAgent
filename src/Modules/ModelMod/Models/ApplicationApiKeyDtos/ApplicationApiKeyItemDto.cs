namespace ModelMod.Models.ApplicationApiKeyDtos;

/// <summary>
/// 应用 ApiKey 列表项
/// </summary>
public class ApplicationApiKeyItemDto
{
    [Key]
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public DateTimeOffset KeyUpdatedTime { get; set; }

    public DateTimeOffset KeyExpiresAt { get; set; }

    public bool IsExpired { get; set; }

    public DateTimeOffset CreatedTime { get; set; }
}