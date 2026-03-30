namespace ModelMod.Models.ApplicationDtos;

/// <summary>
/// 应用访问令牌响应
/// </summary>
public class ApplicationTokenResponseDto
{
    public Guid ApplicationId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(100)]
    public string ClientId { get; set; } = default!;

    public string AccessToken { get; set; } = default!;

    public int ExpiresIn { get; set; }

    public string TokenType { get; set; } = "Bearer";
}