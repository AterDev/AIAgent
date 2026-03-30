namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义FilterDto
/// </summary>
/// <see cref="Application"/>
public class ApplicationFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(100)]
    public string? ClientId { get; set; }
    public bool? IsEnabled { get; set; }

}
