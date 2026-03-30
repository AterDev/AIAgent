namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义DetailDto
/// </summary>
/// <see cref="Application"/>
public class ApplicationDetailDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ClientId { get; set; } = default!;
    public bool HasSecret { get; set; } = true;
    public DateTimeOffset SecretUpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public bool IsEnabled { get; set; } = true;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public Guid TenantId { get; set; }

}
