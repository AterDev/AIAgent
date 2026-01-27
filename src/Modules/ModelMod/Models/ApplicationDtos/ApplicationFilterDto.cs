using Entity.ModelMod;
namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义FilterDto
/// </summary>
/// <see cref="Entity.ModelMod.Application"/>
public class ApplicationFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(100)]
    public string? AccessKey { get; set; }
    public bool? IsEnabled { get; set; }
    
}
