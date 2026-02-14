namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 UpdateDto
/// </summary>
/// <see cref="ApplicationModelPermission"/>
public class ApplicationModelPermissionUpdateDto
{
    public bool? IsEnabled { get; set; }
    public Guid AIModelInfoId { get; set; }
}
