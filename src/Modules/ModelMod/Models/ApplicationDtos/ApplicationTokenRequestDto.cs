namespace ModelMod.Models.ApplicationDtos;

/// <summary>
/// 应用换取访问令牌请求
/// </summary>
public class ApplicationTokenRequestDto
{
    [MaxLength(100)]
    public string ClientId { get; set; } = default!;

    [MaxLength(200)]
    public string ClientSecret { get; set; } = default!;
}