namespace Share.Models;

public class ApiKeyAuthInfo
{
    public Guid ApiKeyId { get; set; }

    public Guid ApplicationId { get; set; }

    public string Name { get; set; } = default!;

    public string ApiKeyName { get; set; } = default!;

    public Guid TenantId { get; set; }

    public string TenantType { get; set; } = nameof(Entity.TenantType.Normal);

    public string KeyFingerprint { get; set; } = default!;

    public string KeyHash { get; set; } = default!;

    public string KeySalt { get; set; } = default!;

    public DateTimeOffset KeyExpiresAt { get; set; }
}