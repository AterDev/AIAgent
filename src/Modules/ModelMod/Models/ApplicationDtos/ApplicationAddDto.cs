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
    [MaxLength(100)]
    public string AccessKey { get; set; } = default!;
    [MaxLength(200)]
    public string SecretKey { get; set; } = default!;
    public bool IsEnabled { get; set; } = true;

}
