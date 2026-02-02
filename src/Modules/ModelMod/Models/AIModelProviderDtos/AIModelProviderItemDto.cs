namespace ModelMod.Models.AIModelProviderDtos;

/// <summary>
/// AI模型提供商ItemDto
/// </summary>
public class AIModelProviderItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? ApiKey { get; set; }
}
