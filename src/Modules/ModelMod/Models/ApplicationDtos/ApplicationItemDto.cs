namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义ItemDto
/// </summary>
/// <see cref="Application"/>
public class ApplicationItemDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    [MaxLength(100)]
    public string AccessKey { get; set; } = default!;
    public bool IsEnabled { get; set; } = true;
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

}
