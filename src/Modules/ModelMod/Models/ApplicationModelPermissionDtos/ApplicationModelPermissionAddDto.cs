namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 AddDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationModelPermission"/>
public class ApplicationModelPermissionAddDto
{
    public Guid ApplicationId { get; set; }
    public Guid ModelProfileId { get; set; }
    public bool IsEnabled { get; set; } = true;
}
