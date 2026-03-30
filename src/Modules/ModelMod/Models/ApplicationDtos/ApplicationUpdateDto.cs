namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义UpdateDto
/// </summary>
/// <see cref="Application"/>
public class ApplicationUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool? IsEnabled { get; set; }

}
