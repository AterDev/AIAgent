namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义DetailDto
/// </summary>
/// <see cref="Entity.ModelMod.Application"/>
public class ApplicationDetailDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(100)]
    public string AccessKey { get; set; } = default!;
    [MaxLength(200)]
    public string SecretKey { get; set; } = default!;
    public bool IsEnabled { get; set; } = true;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public Guid TenantId { get; set; }

}
