namespace ModelMod.Models.ApplicationApiKeyDtos;

/// <summary>
/// 新增应用 ApiKey 返回结果（仅创建时返回明文）
/// </summary>
public class ApplicationApiKeyCredentialResultDto
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string ApplicationName { get; set; } = default!;

    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string ApiKey { get; set; } = default!;

    public DateTimeOffset KeyUpdatedTime { get; set; }

    public DateTimeOffset KeyExpiresAt { get; set; }
}