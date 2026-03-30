namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义AddDto
/// </summary>
/// <see cref="Application"/>
public class ApplicationAddDto
{
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

}
