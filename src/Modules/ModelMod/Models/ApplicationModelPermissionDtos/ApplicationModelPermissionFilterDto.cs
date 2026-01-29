namespace ModelMod.Models.ApplicationModelPermissionDtos;

/// <summary>
/// 应用模型权限 FilterDto
/// </summary>
/// <see cref="Entity.ModelMod.ApplicationModelPermission"/>
public class ApplicationModelPermissionFilterDto : FilterBase
{
    public Guid? ApplicationId { get; set; }
    public Guid? AIModelInfoId { get; set; }
    public bool? IsEnabled { get; set; }
}
