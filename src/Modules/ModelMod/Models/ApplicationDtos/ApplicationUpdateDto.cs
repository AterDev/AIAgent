namespace ModelMod.Models.ApplicationDtos;
/// <summary>
/// 应用定义UpdateDto
/// </summary>
/// <see cref="Entity.ModelMod.Application"/>
public class ApplicationUpdateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? AccessKey { get; set; }
    [MaxLength(200)]
    public string? SecretKey { get; set; }
    public bool? IsEnabled { get; set; }

}
