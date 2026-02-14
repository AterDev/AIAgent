namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 AddDto
/// </summary>
/// <see cref="ApplicationModelPermission"/>
public class ApplicationModelPermissionAddDto
{
    public Guid ApplicationId { get; set; }
    public Guid AIModelInfoId { get; set; }
    public bool IsEnabled { get; set; } = true;
}
