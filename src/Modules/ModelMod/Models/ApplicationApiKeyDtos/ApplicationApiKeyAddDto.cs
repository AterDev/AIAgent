namespace ModelMod.Models.ApplicationApiKeyDtos;

/// <summary>
/// 新增应用 ApiKey
/// </summary>
public class ApplicationApiKeyAddDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public int ApiKeyExpiresInMonths { get; set; } = ApiKeyService.DefaultExpiryMonths;
}